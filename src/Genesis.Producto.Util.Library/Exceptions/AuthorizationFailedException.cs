namespace Genesis.Producto.Util.Library.Exceptions;

[Serializable]
public class AuthorizationFailedException : Exception
{
    public AuthorizationFailedException()
    {
    }

    public AuthorizationFailedException(string? message) : base(message)
    {
    }

    public AuthorizationFailedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
