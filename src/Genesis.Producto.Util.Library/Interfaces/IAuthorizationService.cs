namespace Genesis.Producto.Util.Library.Interfaces;

public interface IAuthorizationService
{
    Task<string> Authorize();
}
