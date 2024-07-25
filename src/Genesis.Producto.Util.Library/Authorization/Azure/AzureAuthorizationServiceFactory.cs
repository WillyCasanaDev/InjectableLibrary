using Genesis.Producto.Util.Library.Interfaces;
using Genesis.Producto.Util.Library.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Genesis.Producto.Util.Library.Authorization.Azure;

public class AzureAuthorizationServiceFactory : IAzureAuthorizationServiceFactory
{
    private readonly List<ClientOptions> _clientOptions;
    private readonly TokenStore _tokenStore;
    private readonly ILogger<AzureAuthorizationServiceFactory> _logger;

    public AzureAuthorizationServiceFactory(
        IOptions<List<ClientOptions>> azureAdOptions,
        TokenStore tokenStore,
        ILogger<AzureAuthorizationServiceFactory> logger)
    {
        _clientOptions = azureAdOptions.Value;
        _tokenStore = tokenStore;
        _logger = logger;
    }

    public AzureAuthorizationService Create(string clientName)
    {
        try
        {
            ClientOptions clientOptions = _clientOptions
                .First(option => option.ClientName == clientName);

            return new AzureAuthorizationService(clientOptions, _tokenStore, _logger);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"No Azure AD client configuration found for client name: {clientName}");
        }
    }
}
