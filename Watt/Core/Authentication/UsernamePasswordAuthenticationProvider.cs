using System;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Core.Authentication;

/// <summary>
/// Handles Username/Password authentication.
/// </summary>
public class UsernamePasswordAuthenticationProvider : IAuthenticationProvider
{
    public AuthenticationMethod Method => AuthenticationMethod.UsernamePassword;

    public async Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment)
    {
        // Direct call not used for username/password; use AuthenticateWithCredentialsAsync
        return await Task.FromResult(AuthenticationResult.Failure(
            "Use AuthenticateWithCredentialsAsync for Username/Password authentication"));
    }

    /// <summary>
    /// Authenticates using username and password credentials.
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateWithCredentialsAsync(
        EnvironmentDetails environment,
        UsernamePasswordCredentials credentials)
    {
        try
        {
            // Create connection string for username/password auth
            var connectionString = $"AuthType=OAuth;Url={environment.OrgUrl};Username={credentials.Username};Password={credentials.Password};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost/";

            using var client = new ServiceClient(connectionString);

            if (!client.IsReady)
            {
                return AuthenticationResult.Failure($"Failed to connect: {client.LastError}");
            }

            // For username/password, we store the credentials themselves (they don't expire like tokens)
            var storedCredentials = new UsernamePasswordCredentials
            {
                EnvironmentId = environment.Id,
                Username = credentials.Username,
                Password = credentials.Password
            };

            return AuthenticationResult.Success(storedCredentials);
        }
        catch (Exception ex)
        {
            return AuthenticationResult.Failure($"Username/Password authentication failed: {ex.Message}");
        }
    }

    public async Task<bool> ValidateAsync(Credentials credentials, EnvironmentDetails environment)
    {
        if (credentials is not UsernamePasswordCredentials userPassCreds)
            return false;

        try
        {
            var connectionString = $"AuthType=OAuth;Url={environment.OrgUrl};Username={userPassCreds.Username};Password={userPassCreds.Password};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost/";

            using var client = new ServiceClient(connectionString);
            return client.IsReady;
        }
        catch
        {
            return false;
        }
    }
}
