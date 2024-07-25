using Microsoft.AspNetCore.Http;
using System;

namespace Genesis.Producto.Util.Library.Exceptions;

[Serializable]
public class ExternalRequestException(int statusCode, string url, string message) : Exception($"{statusCode} - {url} - {message}")
{
    public int StatusCode { get; } = statusCode;
    public string Url { get; } = url;
  
}
