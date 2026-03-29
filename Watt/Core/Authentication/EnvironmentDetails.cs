namespace Watt.Core.Authentication;

/// <summary>
/// Represents details about a Dataverse environment.
/// </summary>
public class EnvironmentDetails
{
    /// <summary>
    /// Unique identifier for this environment configuration.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// User-friendly name for the environment.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The Dataverse organization URL (e.g., https://org.crm.dynamics.com).
    /// </summary>
    public required string OrgUrl { get; set; }

    /// <summary>
    /// The authentication method used for this environment.
    /// </summary>
    public required AuthenticationMethod AuthMethod { get; set; }

    /// <summary>
    /// When the credentials for this environment were last successfully authenticated.
    /// </summary>
    public DateTime? LastAuthenticatedAt { get; set; }

    /// <summary>
    /// Whether the connection is currently valid and authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }
}
