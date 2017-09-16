using System;

namespace Rinjani.Bitflyer
{
    public class SendChildOrderParam
    {
        public SendChildOrderParam(Order order)
        {
            if (order.CashMarginType != CashMarginType.Cash)
            {
                throw new NotImplementedException();
            }

            string productCode;
            switch (order.Symbol)
            {
                case "BTCJPY":
                    productCode = "BTC_JPY";
                    break;
                default:
                    throw new NotImplementedException();
            }

            string childOrderType;
            switch (order.Type)
            {
                case OrderType.Limit:
                    childOrderType = "LIMIT";
                    price = order.Price;
                    break;
                case OrderType.Market:
                    childOrderType = "MARKET";
                    price = 0;
                    break;
                default: throw new NotSupportedException();
            }

            string timeInForce;
            switch (order.TimeInForce)
            {
                case TimeInForce.None:
                    timeInForce = "";
                    break;
                case TimeInForce.Fok:
                    timeInForce = "FOK";
                    break;
                case TimeInForce.Ioc:
                    timeInForce = "IOC";
                    break;
                default: throw new NotSupportedException();
            }

            product_code = productCode;
            child_order_type = childOrderType;
            side = order.Side.ToString().ToUpper();

            size = order.Size;
            time_in_force = timeInForce;
        }

        public string product_code { get; set; }
        public string child_order_type { get; set; }
        public string side { get; set; }
        public decimal price { get; set; }
        public decimal size { get; set; }
        public string time_in_force { get; set; }
    }
}