using Microsoft.PowerPlatform.Dataverse.Client;
using Watt.Core.Authentication;

namespace Watt.Core;

/// <summary>
/// Represents the global application state.
/// </summary>
public class AppState
{
    /// <summary>
    /// The currently selected environment ID.
    /// </summary>
    public string? CurrentEnvironmentId { get; set; }

    /// <summary>
    /// The current active Dataverse connection.
    /// </summary>
    public ServiceClient? Connection { get; set; }

    /// <summary>
    /// Authentication service for managing credentials and environments.
    /// </summary>
    public required AuthenticationService AuthenticationService { get; set; }

    /// <summary>
    /// Connection manager for Dataverse.
    /// </summary>
    public required DataverseConnectionManager ConnectionManager { get; set; }

    /// <summary>
    /// Switches to a different environment and establishes a connection.
    /// </summary>
    public async Task<bool> SwitchEnvironmentAsync(string environmentId)
    {
        var environment = AuthenticationService.GetEnvironment(environmentId);
        if (environment == null)
            return false;

        // Validate credentials exist and are valid
        var isValid = await AuthenticationService.ValidateCredentialsAsync(environmentId);
        if (!isValid)
            return false;

        // Get or create connection
        var connection = await ConnectionManager.GetConnectionAsync(environmentId);
        if (connection == null)
            return false;

        CurrentEnvironmentId = environmentId;
        Connection = connection;
        return true;
    }
}
