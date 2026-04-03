using System.Collections.Generic;
using System.Threading.Tasks;

namespace Watt.Core.Authentication;

/// <summary>
/// Orchestrates authentication and manages Dataverse environment registrations.
/// Authentication is delegated to the Azure CLI — no credentials are stored by Watt.
/// </summary>
public class AuthenticationService : IAsyncDisposable
{
    private readonly CredentialManager _credentialManager;
    private readonly AzureCliAuthenticationProvider _provider;

    public AuthenticationService()
    {
        _credentialManager = new CredentialManager();
        _provider = new AzureCliAuthenticationProvider();
    }

    /// <summary>Loads stored environments from disk.</summary>
    public async Task InitializeAsync()
    {
        await _credentialManager.LoadStoredEnvironmentsAsync();
    }

    /// <summary>Registers a new Dataverse environment.</summary>
    public async Task RegisterEnvironmentAsync(EnvironmentDetails environment)
    {
        await _credentialManager.SaveEnvironmentAsync(environment);
    }

    /// <summary>
    /// Validates that the Azure CLI can obtain a token for the given environment.
    /// Returns false if the user is not logged in or the token cannot be acquired.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string environmentId)
    {
        var environment = _credentialManager.GetEnvironment(environmentId);
        if (environment == null)
            return false;

        return await _provider.ValidateAsync(null, environment);
    }

    public EnvironmentDetails? GetEnvironment(string environmentId) =>
        _credentialManager.GetEnvironment(environmentId);

    public IEnumerable<EnvironmentDetails> GetAllEnvironments() =>
        _credentialManager.GetAllEnvironments();

    public async Task DeleteEnvironmentAsync(string environmentId)
    {
        await _credentialManager.DeleteEnvironmentAsync(environmentId);
    }

    public async ValueTask DisposeAsync()
    {
        await _credentialManager.DisposeAsync();
    }
}
