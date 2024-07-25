using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genesis.Producto.Util.Library.Authorization.Azure;
using Genesis.Producto.Util.Library.Authorization;
using Genesis.Producto.Util.Library.Interfaces;
using Genesis.Producto.Util.Library.Options;
using Genesis.Producto.Util.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Genesis.Producto.Util.Library.Common;

namespace Genesis.Producto.Util.Library;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExternalHttpRequestClientUtility(
              this IServiceCollection services,
              List<ClientOptions> clientOptions,
              ExternalHttpRequestClientOptions? externalHttpRequestClientOptions = null)
    {
        if (externalHttpRequestClientOptions == null)
        {
            externalHttpRequestClientOptions = new ExternalHttpRequestClientOptions
            {
                EnableSSLByPass = false,
                TimeoutInSeconds = 200
            };
        }

        services.AddSingleton<IAzureAuthorizationServiceFactory, AzureAuthorizationServiceFactory>()
                .AddSingleton<IExternalHttpRequestClient, ExternalHttpRequestClient>()
                .AddSingleton<TokenStore>()
                .Configure<List<ClientOptions>>(options =>
                {
                    options.AddRange(clientOptions);
                })
                .Configure<ExternalHttpRequestClientOptions>(options =>
                {
                    options.EnableSSLByPass = externalHttpRequestClientOptions.EnableSSLByPass;
                    options.TimeoutInSeconds = externalHttpRequestClientOptions.TimeoutInSeconds;
                })
                .AddSingleton<IAuthorizationService, AzureAuthorizationService>()
                .AddOptions();
            
             

        return services;
    }
}
