namespace Watt.Core.Authentication;

/// <summary>
/// Represents details about a Dataverse environment.
/// </summary>
public class EnvironmentDetails
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// The Dataverse organization URL (e.g., https://org.crm.dynamics.com).
    /// </summary>
    public required string OrgUrl { get; set; }
}
