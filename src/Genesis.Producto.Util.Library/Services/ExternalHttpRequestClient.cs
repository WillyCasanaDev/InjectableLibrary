using System.Net.Http.Headers;
using System.Text;
using Genesis.Producto.Util.Library.Exceptions;
using Genesis.Producto.Util.Library.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Genesis.Producto.Util.Library.Options;
using Microsoft.Extensions.Options;
using System;
using Genesis.Producto.Util.Library.Common;

namespace Genesis.Producto.Util.Library.Services
{
    public class ExternalHttpRequestClient : IExternalHttpRequestClient
    {
        private readonly IAzureAuthorizationServiceFactory _azureAuthorizationServiceFactory;
        private readonly ILogger<ExternalHttpRequestClient> _logger;
        private readonly ExternalHttpRequestClientOptions _options;

        public ExternalHttpRequestClient(IAzureAuthorizationServiceFactory azureAuthorizationServiceFactory, ILogger<ExternalHttpRequestClient> logger, IOptions<ExternalHttpRequestClientOptions> options)
        {
            _azureAuthorizationServiceFactory = azureAuthorizationServiceFactory;
            _logger = logger;
            _options = options.Value;
     
        }

        public async Task<TResponse> Post<TRequest, TResponse>(TRequest requestDto, string clientName, string baseUrl, string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null)
        {
            if (retryCount.HasValue && sleepDurationProvider.HasValue)
            {
                var retryPolicyExecutor = CreateRetryPolicyExecutor(retryCount.Value, sleepDurationProvider.Value);

                return await retryPolicyExecutor.ExecuteWithRetry<HttpRequestException, TResponse>(
                    () => SendRequest<TResponse>(HttpMethod.Post, requestDto, clientName, baseUrl, apiEndpoint),
                    retryCount.Value, TimeSpan.FromSeconds(sleepDurationProvider.Value),
                    (exception, timespan, retryCount, context) =>
                    {
                        var message = $"Retry {retryCount} from {apiEndpoint} due to {exception.Message}. Waiting {timespan} before next retry.";
                        _logger.LogWarning(message);
                    });
            }

            return await SendRequest<TResponse>(HttpMethod.Post, requestDto, clientName, baseUrl, apiEndpoint);

        }

        public async Task Post<TRequest>(TRequest requestDto, string clientName, string baseUrl, string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null)
        {
            if (retryCount.HasValue && sleepDurationProvider.HasValue)
            {
                var retryPolicyExecutor = CreateRetryPolicyExecutor(retryCount.Value, sleepDurationProvider.Value);

                await retryPolicyExecutor.ExecuteWithRetry<HttpRequestException>(
                    () => SendRequest<object>(HttpMethod.Post, requestDto, clientName, baseUrl, apiEndpoint),
                    retryCount.Value, TimeSpan.FromSeconds(sleepDurationProvider.Value),
                    (exception, timespan, retryCount, context) =>
                    {
                        var message = $"Retry {retryCount} for {apiEndpoint} due to {exception.Message}. Waiting {timespan} before next retry.";
                        _logger.LogWarning(message);
                    });
            }

            await SendRequest<object>(HttpMethod.Post, requestDto, clientName, baseUrl, apiEndpoint);


        }

        public async Task<TResponse> Get<TResponse>(string clientName, string url, string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null)
        {
            if (retryCount.HasValue && sleepDurationProvider.HasValue)
            {
                var retryPolicyExecutor = CreateRetryPolicyExecutor(retryCount.Value, sleepDurationProvider.Value);

                return await retryPolicyExecutor.ExecuteWithRetry<HttpRequestException, TResponse>(
                    () => SendRequest<TResponse>(HttpMethod.Get, null, clientName, url, apiEndpoint),
                    retryCount.Value, TimeSpan.FromSeconds(sleepDurationProvider.Value),
                    (exception, timespan, retryCount, context) =>
                    {
                        var message = $"Retry {retryCount} for {apiEndpoint} due to {exception.Message}. Waiting {timespan} before next retry.";
                        _logger.LogWarning(message);
                    });
            }

            return await SendRequest<TResponse>(HttpMethod.Get, null, clientName, url, apiEndpoint);
        }

        private IRetryPolicyExecutor CreateRetryPolicyExecutor(int retryCount, int sleepDurationProvider)
        {
            return new RetryPolicyExecutor(new RetryPolicyOptions
            {
                RetryCount = retryCount,
                SleepDurationProvider = sleepDurationProvider
            });
        }

        private async Task<TResponse> SendRequest<TResponse>(HttpMethod method, object? requestDto, string clientName, string url, string apiEndpoint)
        {
            var uri = new Uri(url);
            var baseUrl = uri.GetLeftPart(UriPartial.Authority);
            var endPoint = GetEndPoint(uri, apiEndpoint);
            var json = requestDto != null ? JsonConvert.SerializeObject(requestDto) : string.Empty;

            HttpClientHandler handler = _options.EnableSSLByPass
                ? new HttpClientHandler
                {
                    #pragma warning disable S4830 // Suppressing the SonarLint warning about SSL validation
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                    #pragma warning restore S4830
                }
                : new HttpClientHandler();

            using var client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{baseUrl}/"),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutInSeconds)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                if (!string.IsNullOrEmpty(clientName))
                {
                    var authorizationService = _azureAuthorizationServiceFactory.Create(clientName);
                    var token = await authorizationService.Authorize();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response;
                if (method == HttpMethod.Post)
                {
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(endPoint, stringContent);
                }
                else
                {
                    response = await client.GetAsync(endPoint);
                }

                var result = await HandleResponse<TResponse>(response, baseUrl, apiEndpoint)
                             ?? throw new ExternalRequestException((int)response.StatusCode, $"{baseUrl}{endPoint}", "Response is null");

                return result;
            }
            catch (HttpRequestException e)
            {
                var message = $"HttpRequestException - url: {baseUrl}{endPoint} - body: {json}";
                _logger.LogError(e, message);
                throw new ExternalRequestException(0, $"{baseUrl}{endPoint}", $"Connection error: {e.Message}");
            }
            catch (Exception e)
            {
                var message = $"{e.GetType().Name} - url: {baseUrl}{endPoint} - body: {json}";
                _logger.LogError(e, message);
                throw new ExternalRequestException(0, $"{baseUrl}{endPoint}", e.Message);
            }
        }

        private static string GetEndPoint(Uri uri, string apiEndpoint)
        {
            var pathAndQuery = uri.PathAndQuery; 
            return string.IsNullOrEmpty(pathAndQuery.Trim()) || pathAndQuery.Equals("/")
                ? apiEndpoint
                : $"{pathAndQuery}{apiEndpoint}".Replace("//", "/");
        }

        private async Task<TResponse?> HandleResponse<TResponse>(HttpResponseMessage response, string baseUrl, string apiEndpoint)
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? "URL not available";

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<TResponse>(jsonResult);
                    return result;
                }
                catch (JsonSerializationException jsonEx)
                {
                    throw new ExternalRequestException((int)response.StatusCode, requestUri, $"Failed to deserialize success response: {jsonEx.Message} Raw response: {jsonResult}");
                }
            }

            return HandleErrorResponse<TResponse>(jsonResult, response, baseUrl, apiEndpoint, requestUri);
        }

        private TResponse HandleErrorResponse<TResponse>(string jsonResult, HttpResponseMessage response, string baseUrl, string apiEndpoint, string requestUri)
        {
            try
            {
           
                var errorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase} - URL: {baseUrl}{apiEndpoint}";
             
                if (!string.IsNullOrEmpty(jsonResult))
                {
                    errorMessage += $" - Response: {jsonResult}";
                }

         
                throw new ExternalRequestException((int)response.StatusCode, requestUri, errorMessage);
            }
            catch (JsonSerializationException jsonEx)
            {
                throw new ExternalRequestException((int)response.StatusCode, requestUri, $"Failed to deserialize error response: {jsonEx.Message} - Raw response: {jsonResult}");
            }
        }
    }
}
