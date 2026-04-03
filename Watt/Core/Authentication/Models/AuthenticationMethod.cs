namespace Watt.Core.Authentication;

/// <summary>
/// Enum representing supported authentication methods for Dataverse connections.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Authentication delegated to the Azure CLI (az login).
    /// No credentials are stored by Watt; the CLI manages the token.
    /// </summary>
    AzureCli
}
