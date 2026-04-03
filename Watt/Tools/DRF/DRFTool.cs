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

    public async Task<List<EntityMetadata>> GetAllEntitiesAsync()
    {
        if (AppState.Connection == null)
            throw new InvalidOperationException("No active connection.");

        var request = new RetrieveAllEntitiesRequest
        {
            EntityFilters = EntityFilters.Entity,
            RetrieveAsIfPublished = true
        };

        var response = (RetrieveAllEntitiesResponse)AppState.Connection.Execute(request);
        return [.. response.EntityMetadata.Select(em => new EntityMetadata
        {
            LogicalName = em.LogicalName,
            DisplayName = em.DisplayName
        })];
    }

}