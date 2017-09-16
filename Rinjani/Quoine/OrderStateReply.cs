using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinjani.Quoine
{
    public class OrderStateReply
    {
        public string id { get; set; }
        public string order_type { get; set; }
        public decimal quantity { get; set; }
        public decimal disc_quantity { get; set; }
        public decimal iceberg_total_quantity { get; set; }
        public string side { get; set; }
        public decimal filled_quantity { get; set; }
        public decimal price { get; set; }
        public int created_at { get; set; }
        public int updated_at { get; set; }
        public string status { get; set; }
        public int leverage_level { get; set; }
        public string source_exchange { get; set; }
        public int product_id { get; set; }
        public string product_code { get; set; }
        public string funding_currency { get; set; }
        public string currency_pair_code { get; set; }
        public decimal order_fee { get; set; }
        public List<Execution> executions { get; set; }

        public void SetOrder(Order order)
        {
            order.BrokerOrderId = id;
            order.FilledSize = filled_quantity;
            order.CreationTime = Util.UnixTimeStampToDateTime(created_at);
            if (order.FilledSize == order.Size)
            {
                order.Status = OrderStatus.Filled;
            }
            else if (order.FilledSize > 0)
            {
                order.Status = OrderStatus.PartiallyFilled;
            }
            order.Executions = executions.Select(x => new Rinjani.Execution(order)
            {
                Price = x.price,
                Size = x.quantity,
                ExecTime = Util.UnixTimeStampToDateTime(x.created_at)
            }).ToList();
            order.LastUpdated = DateTime.Now;
        }

        public class Execution
        {
            public int id { get; set; }
            public decimal quantity { get; set; }
            public decimal price { get; set; }
            public string taker_side { get; set; }
            public string my_side { get; set; }
            public int created_at { get; set; }
        }
    }
}