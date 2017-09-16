using Rinjani.Bitflyer;
using Rinjani.Quoine;

namespace Rinjani
{
    public class OrderState
    {
        public OrderState(ChildOrdersReply b)
        {
            OrderId = b.child_order_acceptance_id;
            Done = b.outstanding_size == 0;
            PendingSize = b.outstanding_size;
        }

        public OrderState(OrderStateReply b)
        {
            OrderId = b.id;
            Done = b.status == "filled";
            PendingSize = b.quantity - b.filled_quantity;
        }

        public OrderState()
        {
        }

        public string OrderId { get; set; }
        public bool Done { get; set; }
        public decimal PendingSize { get; set; }

        public override string ToString()
        {
            return Done
                ? $"Id: {OrderId}, Done: {Done}"
                : $"Id: {OrderId}, Done: {Done}, Pending: {PendingSize}";
        }
    }
}