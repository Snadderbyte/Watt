using System;

namespace Watt.Core.Authentication;

/// <summary>
/// Base class for storing authentication credentials.
/// </summary>
public abstract class Credentials
{
    /// <summary>
    /// The authentication method this credential represents.
    /// </summary>
    public abstract AuthenticationMethod Method { get; }

    /// <summary>
    /// The environment ID these credentials are for.
    /// </summary>
    public required string EnvironmentId { get; set; }

    /// <summary>
    /// When these credentials were created/saved.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// OAuth credentials for interactive login.
/// </summary>
public class OAuthCredentials : Credentials
{
    public override AuthenticationMethod Method => AuthenticationMethod.OAuth;

    /// <summary>
    /// The access token obtained from the OAuth provider.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Optional refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Client Secret credentials for Service Principal authentication.
/// </summary>
public class ClientSecretCredentials : Credentials
{
    public override AuthenticationMethod Method => AuthenticationMethod.ClientSecret;

    /// <summary>
    /// The Azure AD tenant ID.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// The application (client) ID registered in Azure AD.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The client secret (should be encrypted when stored).
    /// </summary>
    public required string ClientSecret { get; set; }
}

/// <summary>
/// Username/Password credentials for basic authentication.
/// </summary>
public class UsernamePasswordCredentials : Credentials
{
    public override AuthenticationMethod Method => AuthenticationMethod.UsernamePassword;

    /// <summary>
    /// The username for authentication.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password (should be encrypted when stored).
    /// </summary>
    public required string Password { get; set; }
}
