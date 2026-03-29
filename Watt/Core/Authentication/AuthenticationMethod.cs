namespace Watt.Core.Authentication;

/// <summary>
/// Enum representing supported authentication methods for Dataverse connections.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// OAuth-based interactive login for personal and work accounts.
    /// Requires user interaction for sign-in.
    /// </summary>
    OAuth,

    /// <summary>
    /// Service Principal authentication using Client ID and Client Secret.
    /// Non-interactive, suitable for automated processes.
    /// </summary>
    ClientSecret,

    /// <summary>
    /// Username and Password authentication.
    /// </summary>
    UsernamePassword
}
