using Genesis.Producto.Util.Library;
using Genesis.Producto.Util.Library.Interfaces;
using Genesis.Producto.Util.Library.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExternalHttpRequestClientTest
{
    static class Program
    {
        static async Task Main(string[] args)
        {

            try
            {
                var baseURL = $"https://genkin-emea-uat.prosegur.dev/accreditation";

                var clientOptions = new List<ClientOptions>
            {
                //To set valid values to test Get and Post methods
                new ClientOptions
                {
                    //uat
                    ClientName = "ClientNameFake",
                    ClientId = "ClientIdFake",
                    ClientSecret = "ClientSecretFake",
                    Instance = "InstanceFake",
                    Scope = "ScopeFake",
                    TenantId = "TenantIdFake"
                }
            };
                var externalHttpRequestClientOptions = new ExternalHttpRequestClientOptions
                {
                    EnableSSLByPass = true,
                    TimeoutInSeconds = 300
                };
             

                var serviceProvider = new ServiceCollection()
                    .AddLogging(configure => configure.AddConsole())
                    .AddExternalHttpRequestClientUtility(clientOptions, externalHttpRequestClientOptions)
                    .BuildServiceProvider();



                var client = serviceProvider.GetService<IExternalHttpRequestClient>();
                if (client == null)
                {
                    Console.WriteLine("Failed to retrieve the ExternalHttpRequestClient service.");
                    return;
                }

                // Test GET
                var getResponse = await client.Get<object>("SettlementReads", baseURL, "/api/v1/Settlements/6e920119-167e-41dd-bdbc-e5909f39fe69");

                Console.WriteLine("Get Response: " + getResponse);


            }
            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);
            }



         
        }
    
    }
}
