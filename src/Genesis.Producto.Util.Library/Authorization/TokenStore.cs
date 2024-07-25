namespace Genesis.Producto.Util.Library.Authorization;

public class TokenStore
{
    public Dictionary<string, Token> Tokens { get; } = new();
}

public class Token
{
    public string? AccessToken { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOn;
}
