namespace Watt.Core.Authentication;

/// <summary>
/// Represents details about a Dataverse environment.
/// </summary>
public class EnvironmentDetails
{
    /// <summary>
    /// A unique identifier for the environment. This can be a GUID or any string that uniquely identifies the environment. It is used to associate credentials with the correct environment.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// A user-friendly name for the environment, e.g. "Contoso Production". This is not used for authentication but can help users identify their environments in the UI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Indicates whether these credentials are currently active.
    /// </summary>
    public required bool IsActive { get; set; } = false;

    /// <summary>
    /// The Dataverse organization URL (e.g., https://org.crm.dynamics.com).
    /// </summary>
    public required string OrgUrl { get; set; }
}
