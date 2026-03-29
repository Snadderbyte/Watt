using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Core.Authentication;

/// <summary>
/// Manages connections to Dataverse organizations.
/// Handles creating and caching Dataverse ServiceClient instances.
/// </summary>
public class DataverseConnectionManager : IAsyncDisposable
{
    private readonly AuthenticationService _authService;
    private readonly Dictionary<string, ServiceClient> _connections;

    public DataverseConnectionManager(AuthenticationService authService)
    {
        _authService = authService;
        _connections = new Dictionary<string, ServiceClient>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets or creates a Dataverse connection for the specified environment.
    /// </summary>
    public async Task<ServiceClient?> GetConnectionAsync(string environmentId)
    {
        // Return cached connection if available and valid
        if (_connections.TryGetValue(environmentId, out var existingClient))
        {
            if (existingClient.IsReady)
                return existingClient;
            else
                _connections.Remove(environmentId);
        }

        // Get environment and credentials
        var environment = _authService.GetEnvironment(environmentId);
        if (environment == null)
            return null;

        var credentials = _authService.GetCredentials(environmentId);
        if (credentials == null)
            return null;

        // Create new connection
        try
        {
            ServiceClient client = credentials switch
            {
                OAuthCredentials oauth => new ServiceClient(
                    new Uri(environment.OrgUrl),
                    async _ => await Task.FromResult(oauth.AccessToken),
                    true),
                UsernamePasswordCredentials userPass => new ServiceClient(
                    $"AuthType=OAuth;Url={environment.OrgUrl};Username={userPass.Username};Password={userPass.Password};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost/"),
                _ => throw new NotSupportedException($"Credentials type {credentials.GetType().Name} not supported")
            };

            if (client.IsReady)
            {
                _connections[environmentId] = client;
                return client;
            }
            else
            {
                return null; // Connection failed
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating Dataverse connection: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Closes a specific connection.
    /// </summary>
    public void CloseConnection(string environmentId)
    {
        if (_connections.TryGetValue(environmentId, out var client))
        {
            try
            {
                client.Dispose();
            }
            catch { }

            _connections.Remove(environmentId);
        }
    }

    /// <summary>
    /// Closes all connections.
    /// </summary>
    public void CloseAllConnections()
    {
        foreach (var client in _connections.Values)
        {
            try
            {
                client.Dispose();
            }
            catch { }
        }

        _connections.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        CloseAllConnections();
        await Task.CompletedTask;
    }
}
