using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
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
    public async Task<List<EntityMetadata>> GetAllEntitiesAsync()
    {
        if (AppState.ServiceClient == null)
            throw new InvalidOperationException("No active connection.");

        var request = new RetrieveAllEntitiesRequest
        {
            EntityFilters = EntityFilters.Entity,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveAllEntitiesResponse)AppState.ServiceClient.Execute(request);
        return [.. response.EntityMetadata.Select(em => new EntityMetadata
        {
            LogicalName = em.LogicalName,
            DisplayName = em.DisplayName
        })];
    }

    public async Task<EntityMetadata> GetEntityMetadataAsync(string logicalName)
    {
        if (AppState.ServiceClient == null)
            throw new InvalidOperationException("No active connection.");

        var request = new RetrieveEntityRequest
        {
            LogicalName = logicalName,
            EntityFilters = EntityFilters.All,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveEntityResponse)AppState.ServiceClient.Execute(request);
        var em = response.EntityMetadata;
        return new EntityMetadata
        {
            LogicalName = em.LogicalName,
            DisplayName = em.DisplayName
        };
    }

}