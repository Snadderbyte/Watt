using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Core.Authentication;

/// <summary>
/// Manages Dataverse ServiceClient instances, one per environment.
/// Tokens are obtained on each connection call via the Azure CLI credential —
/// no token caching or storage is done here.
/// </summary>
public class DataverseConnectionManager(CredentialManager credentialManager) : IAsyncDisposable
{
    private readonly CredentialManager _credentialManager = credentialManager;
    private readonly Dictionary<string, ServiceClient> _connections = new(StringComparer.OrdinalIgnoreCase);
    private readonly AzureCliCredential _credential = new();

    /// <summary>
    /// Gets or creates a Dataverse ServiceClient for the given environment.
    /// Returns null if the environment is not registered or the connection fails.
    /// </summary>
    public async Task<ServiceClient?> GetConnectionAsync(string environmentId)
    {
        var environment = _credentialManager.GetEnvironment(environmentId);
        if (environment == null)
            return null;

        try
        {
            var scope = BuildScope(environment.OrgUrl);
            var client = new ServiceClient(
                new Uri(environment.OrgUrl),
                async _ =>
                {
                    var token = await _credential.GetTokenAsync(new TokenRequestContext([scope]));
                    return token.Token;
                },
                true);

            if (client.IsReady)
            {
                _connections[environmentId] = client;
                return client;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating Dataverse connection: {ex.Message}");
            return null;
        }
    }

    private void CloseAllConnections()
    {
        foreach (var client in _connections.Values)
            try { client.Dispose(); } catch { }

        _connections.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        CloseAllConnections();
        await Task.CompletedTask;
    }

    internal static string BuildScope(string orgUrl) =>
    $"{orgUrl.TrimEnd('/')}/.default";
}
