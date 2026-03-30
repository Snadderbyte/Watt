using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Core.Authentication;

/// <summary>
/// Handles OAuth authentication for personal and work accounts.
/// </summary>
public class OAuthAuthenticationProvider : IAuthenticationProvider
{
    public AuthenticationMethod Method => AuthenticationMethod.OAuth;

    private const string ClientId = "53aebc6f-46b8-4d08-9829-90234d17fd9e";
    private const string TenantId = "organizations"; // Multi-tenant
    private const string Scopes = "https://org.crm.dynamics.com/.default";

    public async Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment)
    {
        try
        {
            var authority = $"https://login.microsoftonline.com/{TenantId}";
            
            var publicClientApp = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(authority)
                .WithRedirectUri("http://localhost")
                .Build();

            Microsoft.Identity.Client.AuthenticationResult authResult;
            
            try
            {
                // Try silent login first
                var accounts = await publicClientApp.GetAccountsAsync();
                authResult = await publicClientApp.AcquireTokenSilent(
                    new[] { Scopes },
                    accounts.FirstOrDefault()
                ).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Interactive login required
                authResult = await publicClientApp.AcquireTokenInteractive(
                    new[] { Scopes }
                ).ExecuteAsync();
            }

            var credentials = new OAuthCredentials
            {
                EnvironmentId = environment.Id,
                AccessToken = authResult.AccessToken,
                ExpiresAt = authResult.ExpiresOn.UtcDateTime,
                RefreshToken = null
            };

            return AuthenticationResult.Success(credentials);
        }
        catch (Exception ex)
        {
            return AuthenticationResult.Failure($"OAuth authentication failed: {ex.Message}");
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
