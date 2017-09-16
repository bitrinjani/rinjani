namespace Rinjani
{
    public class BitflyerOrderStateReply
    {
        public string id { get; set; }
        public string child_order_id { get; set; }
        public string product_code { get; set; }
        public string side { get; set; }
        public string child_order_type { get; set; }
        public decimal price { get; set; }
        public decimal average_price { get; set; }
        public decimal size { get; set; }
        public string child_order_state { get; set; }
        public string expire_date { get; set; }
        public string child_order_date { get; set; }
        public string child_order_acceptance_id { get; set; }
        public decimal outstanding_size { get; set; }
        public decimal cancel_size { get; set; }
        public decimal executed_size { get; set; }
        public decimal total_commission { get; set; }
    }
}