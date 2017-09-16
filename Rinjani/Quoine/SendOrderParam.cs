using System;

namespace Rinjani.Quoine
{
    public class SendOrderParam
    {
        public SendOrderParam(Order order)
        {
            int productId;
            switch (order.Symbol)
            {
                case "BTCJPY":
                    productId = 5;
                    break;
                default:
                    throw new NotImplementedException();
            }

            string orderType;
            switch (order.Type)
            {
                case OrderType.Limit:
                    orderType = "limit";
                    price = order.Price;
                    break;
                case OrderType.Market:
                    orderType = "market";
                    price = 0;
                    break;
                default:
                    throw new NotSupportedException();
            }

            string orderDirection;
            switch (order.CashMarginType)
            {
                case CashMarginType.Cash:
                    orderDirection = null;
                    break;
                case CashMarginType.NetOut:
                    orderDirection = "netout";
                    break;
                default:
                    throw new NotImplementedException();
            }

            order_type = orderType;
            product_id = productId;
            side = order.Side.ToString().ToLower();
            quantity = order.Size;
            leverage_level = order.LeverageLevel;
            order_direction = orderDirection;
        }

        public string order_type { get; set; }
        public int product_id { get; set; }
        public string side { get; set; }
        public decimal quantity { get; set; }
        public decimal price { get; set; }
        public int leverage_level { get; set; }
        public string order_direction { get; set; }
    }
}