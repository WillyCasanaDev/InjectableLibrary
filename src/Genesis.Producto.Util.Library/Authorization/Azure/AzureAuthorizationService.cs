using Genesis.Producto.Util.Library.Exceptions;
using Genesis.Producto.Util.Library.Options;
using Genesis.Producto.Util.Library.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Genesis.Producto.Util.Library.Authorization.Azure;

public class AzureAuthorizationService(
    ClientOptions clientOptions,
    TokenStore tokenStore,
    ILogger<AzureAuthorizationServiceFactory> logger)
    : IAuthorizationService
{
    public async Task<string> Authorize()
    {
        if (tokenStore.Tokens.Count > 0 && tokenStore.Tokens.ContainsKey(clientOptions.ClientName) &&
            !tokenStore.Tokens[clientOptions.ClientName].IsExpired)
        {
            return tokenStore.Tokens[clientOptions.ClientName].AccessToken!;
        }

        IConfidentialClientApplication? app = ConfidentialClientApplicationBuilder
            .Create(clientOptions.ClientId)
            .WithClientSecret(clientOptions.ClientSecret)
            .WithAuthority($"{clientOptions.Instance}/{clientOptions.TenantId}/v2.0")
            .Build();

        string[] scopes = new[] { clientOptions.Scope };

        AuthenticationResult result;
        try
        {
            result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            tokenStore.Tokens[clientOptions.ClientName] = new Token
            {
                AccessToken = result.AccessToken, ExpiresOn = result.ExpiresOn
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw new AuthorizationFailedException("Authentication failed.", ex);
        }

        return result.AccessToken;
    }
}
