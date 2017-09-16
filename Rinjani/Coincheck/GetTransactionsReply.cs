using System.Collections.Generic;

namespace Rinjani.Coincheck
{
    public class TransactionsReply
    {
        public bool success { get; set; }
        public Pagination pagination { get; set; }
        public List<Datum> data { get; set; }

        public class Pagination
        {
            public int limit { get; set; }
            public string order { get; set; }
            public string starting_after { get; set; }
            public string ending_before { get; set; }
        }

        public class Funds
        {
            public decimal btc { get; set; }
            public decimal jpy { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string order_id { get; set; }
            public string created_at { get; set; }
            public Funds funds { get; set; }
            public string pair { get; set; }
            public decimal rate { get; set; }
            public string fee_currency { get; set; }
            public string fee { get; set; }
            public string liquidity { get; set; }
            public string side { get; set; }
        }
    }
}