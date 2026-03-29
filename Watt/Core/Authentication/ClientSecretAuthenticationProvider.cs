using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Core.Authentication;

/// <summary>
/// Handles Service Principal (Client ID + Client Secret) authentication.
/// </summary>
public class ClientSecretAuthenticationProvider : IAuthenticationProvider
{
    public AuthenticationMethod Method => AuthenticationMethod.ClientSecret;

    public async Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment)
    {
        // In a real implementation, this would be called with the credentials
        // For now, return a failure indicating that direct auth flow isn't called here
        return await Task.FromResult(AuthenticationResult.Failure(
            "Use AuthenticateWithCredentialsAsync for Client Secret authentication"));
    }

    /// <summary>
    /// Authenticates using stored client secret credentials.
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateWithCredentialsAsync(
        EnvironmentDetails environment,
        ClientSecretCredentials credentials)
    {
        try
        {
            var authority = $"https://login.microsoftonline.com/{credentials.TenantId}";

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(credentials.ClientId)
                .WithClientSecret(credentials.ClientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            var authResult = await confidentialClientApp.AcquireTokenForClient(
                new[] { "https://org.crm.dynamics.com/.default" }
            ).ExecuteAsync();

            var oauthCredentials = new OAuthCredentials
            {
                EnvironmentId = environment.Id,
                AccessToken = authResult.AccessToken,
                ExpiresAt = authResult.ExpiresOn.UtcDateTime,
                RefreshToken = null
            };

            return AuthenticationResult.Success(oauthCredentials);
        }
        catch (Exception ex)
        {
            return AuthenticationResult.Failure($"Client Secret authentication failed: {ex.Message}");
        }
    }

    public async Task<bool> ValidateAsync(Credentials credentials, EnvironmentDetails environment)
    {
        if (credentials is not OAuthCredentials oauthCreds)
            return false;

        // Check if token is expired or about to expire
        if (oauthCreds.ExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            return false;

        try
        {
            // Try to use the token with Dataverse
            using var client = new ServiceClient(
                new Uri(environment.OrgUrl),
                async _ => await Task.FromResult(oauthCreds.AccessToken),
                true);
            return client.IsReady;
        }
        catch
        {
            return false;
        }
    }
}
