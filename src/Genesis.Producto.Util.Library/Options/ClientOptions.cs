    namespace Genesis.Producto.Util.Library.Options;

    public class ClientOptions
    {
        public string ClientName { get; set; } = null!;
        public string Instance { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string TenantId { get; set; } = null!;
        public string Scope { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool AllowWebApiToBeAuthorizedByACL { get; set; } = true;
    }
