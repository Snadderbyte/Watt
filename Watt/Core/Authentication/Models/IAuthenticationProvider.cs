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
    /// Validates if credentials are still usable for the given environment.
    /// For providers that do not store credentials (e.g. AzureCli), pass null.
    /// </summary>
    Task<bool> ValidateAsync(Credentials? credentials, EnvironmentDetails environment);
}

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public class AuthenticationResult
{
    public bool IsSuccessful { get; set; }
    public Credentials? Credentials { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthenticationResult Success(Credentials? credentials = null) =>
        new() { IsSuccessful = true, Credentials = credentials };

    public static AuthenticationResult Failure(string errorMessage) =>
        new() { IsSuccessful = false, ErrorMessage = errorMessage };
}
