using System;
using System.Threading.Tasks;

namespace Watt.Core.Authentication;

/// <summary>
/// Interface for authentication providers that handle different authentication methods.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// The authentication method this provider handles.
    /// </summary>
    AuthenticationMethod Method { get; }

    /// <summary>
    /// Authenticates with Dataverse and returns credentials.
    /// </summary>
    /// <param name="environment">The environment to authenticate with.</param>
    /// <returns>Authentication result with credentials or error information.</returns>
    Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment);

    /// <summary>
    /// Validates if the provided credentials are still valid.
    /// </summary>
    /// <param name="credentials">The credentials to validate.</param>
    /// <param name="environment">The environment to validate against.</param>
    /// <returns>True if credentials are valid, false otherwise.</returns>
    Task<bool> ValidateAsync(Credentials credentials, EnvironmentDetails environment);
}

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Whether authentication was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// The obtained credentials (null if not successful).
    /// </summary>
    public Credentials? Credentials { get; set; }

    /// <summary>
    /// Error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static AuthenticationResult Success(Credentials credentials) => 
        new() { IsSuccessful = true, Credentials = credentials };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static AuthenticationResult Failure(string errorMessage) => 
        new() { IsSuccessful = false, ErrorMessage = errorMessage };
}
