using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Watt.Core.Authentication;

/// <summary>
/// Authentication provider that delegates to the Azure CLI (az login).
/// No credentials are stored by Watt; the CLI manages the token lifecycle.
/// </summary>
public class AzureCliAuthenticationProvider : IAuthenticationProvider
{
    private readonly AzureCliCredential _credential = new();

    public AuthenticationMethod Method => AuthenticationMethod.AzureCli;

    public async Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment)
    {
        try
        {
            var tokenContext = new TokenRequestContext(new[] { BuildScope(environment.OrgUrl) });
            await _credential.GetTokenAsync(tokenContext);
            return AuthenticationResult.Success();
        }
        catch (Exception ex)
        {
            return AuthenticationResult.Failure(
                $"Azure CLI authentication failed: {ex.Message}\n" +
                "Make sure you are logged in with 'az login'.");
        }
    }

    public async Task<bool> ValidateAsync(Credentials? credentials, EnvironmentDetails environment)
    {
        try
        {
            var tokenContext = new TokenRequestContext(new[] { BuildScope(environment.OrgUrl) });
            await _credential.GetTokenAsync(tokenContext);
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static string BuildScope(string orgUrl) =>
        $"{orgUrl.TrimEnd('/')}/.default";
}
