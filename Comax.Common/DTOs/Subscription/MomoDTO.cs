namespace Comax.Common.DTOs.Subscription
{

    public class MomoIPNRequest
    {
        public string partnerCode { get; set; } = null!;
        public string orderId { get; set; } = null!;
        public string requestId { get; set; } = null!;
        public long amount { get; set; }
        public string orderInfo { get; set; } = null!;
        public string orderType { get; set; } = null!;
        public string transId { get; set; } = null!;
        public int resultCode { get; set; }
        public string message { get; set; } = null!;
        public string payType { get; set; } = null!;
        public string signature { get; set; } = null!;
    }
}