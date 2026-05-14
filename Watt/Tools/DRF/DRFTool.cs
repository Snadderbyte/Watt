using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Watt.Core;



namespace Watt.Tools.DRF;

internal class DrfTool
{
    public AppState AppState { get; set; }

    public DrfTool(AppState appState)
    {
        AppState = appState;
    }

    /// <summary>
    /// Retrieves metadata for all entities in the connected Dataverse environment. This method uses the RetrieveAllEntitiesRequest to fetch basic information about each entity, including its logical name and display name. The results are returned as a list of EntityMetadata objects, which can be used to populate the UI and allow users to select an entity for duplicate row analysis.
    /// </summary>
    /// <returns>A list of EntityMetadata objects representing all entities in the environment.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is no active connection.</exception>
    public async Task<List<EntityMetadata>> GetEntitiesAsync(string searchTerm = "")
    {
        if (AppState.ServiceClient == null)
            throw new InvalidOperationException("No active connection.");

        var request = new RetrieveAllEntitiesRequest
        {
            EntityFilters = EntityFilters.Entity,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveAllEntitiesResponse)AppState.ServiceClient.Execute(request);
        return [.. response.EntityMetadata
            .Where(em => string.IsNullOrEmpty(searchTerm) || em.LogicalName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Select(em => new EntityMetadata
        {
            LogicalName = em.LogicalName,
            DisplayName = em.DisplayName
        })];
    }

    public async Task<List<AttributeMetadata>> GetAttributesAsync(string entityLogicalName)
    {
        if (AppState.ServiceClient == null)
            throw new InvalidOperationException("No active connection.");

        var request = new RetrieveEntityRequest
        {
            LogicalName = entityLogicalName,
            EntityFilters = EntityFilters.Attributes,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveEntityResponse)AppState.ServiceClient.Execute(request);
        return [.. response.EntityMetadata.Attributes];
    }

    public async Task<List<DuplicateGroup>> FindDuplicatesAsync(string entityLogicalName, List<string> attributeLogicalNames)
    {
        if (AppState.ServiceClient == null)
            throw new InvalidOperationException("No active connection.");

        var query = new QueryExpression(entityLogicalName)
        {
            ColumnSet = new ColumnSet([.. attributeLogicalNames]),
            PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 },
        };

        var allRecords = new List<Entity>();
        EntityCollection result;
        do
        {
            result = await Task.Run(() => AppState.ServiceClient.RetrieveMultiple(query));
            allRecords.AddRange(result.Entities);
            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = result.PagingCookie;
        } while (result.MoreRecords);

        return [.. allRecords
            .GroupBy(entity => string.Join("|", attributeLogicalNames.Select(attribute =>
                entity.Contains(attribute) ? GetAttributeStringValue(entity[attribute]) : "")))
            .Where(group => group.Count() > 1)
            .Select(group => new DuplicateGroup
            {
                AttributeValues = attributeLogicalNames.ToDictionary(
                    attribute => attribute,
                    attribute => group.First().Contains(attribute) ? GetAttributeStringValue(group.First()[attribute]) : ""),
                Records = [.. group],
            })];
    }

    public static string GetAttributeStringValue(object? value) => value switch
    {
        OptionSetValue osv => osv.Value.ToString(),
        OptionSetValueCollection osvc => string.Join(", ", osvc.Select(o => o.Value)),
        EntityReference er => er.Name ?? er.Id.ToString(),
        Money money => money.Value.ToString(),
        _ => value?.ToString() ?? "",
    };

    public class DuplicateGroup
    {
        public Dictionary<string, string> AttributeValues { get; set; } = new();
        public List<Entity> Records { get; set; } = new();
        public int DuplicateCount => Records.Count;
    }
}