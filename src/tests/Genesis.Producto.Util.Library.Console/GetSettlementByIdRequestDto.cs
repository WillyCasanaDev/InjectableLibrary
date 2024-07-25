

namespace ExternalHttpRequestClientTest
{
    public class GetSettlementByIdRequestDto
    {
        public string SettlementId { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public bool ForceExtraData { get; set; } = false;
    }
}
