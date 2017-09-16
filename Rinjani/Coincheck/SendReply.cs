namespace Rinjani.Coincheck
{
    public class SendReply
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string OrderType { get; set; }
        public decimal StopLossRate { get; set; }
        public string Pair { get; set; }
        public string CreatedAt { get; set; }
    }
}