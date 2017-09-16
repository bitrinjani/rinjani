namespace Rinjani.Quoine
{
    public class SendReply
    {
        public string Id { get; set; }
        public string OrderType { get; set; }
        public string Quantity { get; set; }
        public string Disc_quantity { get; set; }
        public string Iceberg_total_quantity { get; set; }
        public string Side { get; set; }

        public string Filled_quantity { get; set; }

        public string Price { get; set; }

        public string created_at { get; set; }

        public string updated_at { get; set; }

        public string status { get; set; }

        public string leverage_level { get; set; }

        public string source_exchange { get; set; }

        public string product_id { get; set; }

        public string product_code { get; set; }

        public string funding_currency { get; set; }

        public string currency_pair_code { get; set; }

        public string order_fee { get; set; }
    }
}