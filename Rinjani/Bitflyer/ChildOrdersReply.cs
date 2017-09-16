using System;

namespace Rinjani.Bitflyer
{
    public class ChildOrdersReply
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

        public void PopulateOrderProperties(Order order)
        {
            order.FilledSize = executed_size;
            if (child_order_state == "CANCELED")
            {
                order.Status = OrderStatus.Canceled;
            }
            else if (child_order_state == "EXPIRED")
            {
                order.Status = OrderStatus.Expired;
            }
            else if (order.FilledSize == order.Size)
            {
                order.Status = OrderStatus.Filled;
            }
            else if (order.FilledSize > 0)
            {
                order.Status = OrderStatus.PartiallyFilled;
            }

            order.LastUpdated = DateTime.Now;
        }
    }
}