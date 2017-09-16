using System.Collections.Generic;

namespace Rinjani.Coincheck
{
    public class OpenOrdersReply
    {
        public bool success { get; set; }
        public List<Order> orders { get; set; }

        public class Order
        {
            public string id { get; set; }
            public string order_type { get; set; }
            public decimal? rate { get; set; }
            public string pair { get; set; }
            public decimal? pending_amount { get; set; }
            public decimal? pending_market_buy_amount { get; set; }
            public decimal? stop_loss_rate { get; set; }
            public string created_at { get; set; }
        }
    }
}