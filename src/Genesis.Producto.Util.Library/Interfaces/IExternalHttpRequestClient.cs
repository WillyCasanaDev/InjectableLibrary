namespace Genesis.Producto.Util.Library.Interfaces;

public interface IExternalHttpRequestClient
{
    Task<TResponse> Post<TRequest, TResponse>(TRequest requestDto, string clientName, string baseUrl,
        string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null);

    Task Post<TRequest>(TRequest requestDto, string clientName, string baseUrl,
        string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null);

    Task<TResponse> Get<TResponse>(string clientName, string url, string apiEndpoint, int? retryCount = null, int? sleepDurationProvider = null);
}
