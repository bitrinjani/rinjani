namespace Rinjani
{
    public class QuoteKey
    {
        public QuoteKey(Broker broker, QuoteSide side, decimal price)
        {
            Broker = broker;
            Side = side;
            Price = price;
        }

        public QuoteKey()
        {
        }

        public Broker Broker { get; }
        public QuoteSide Side { get; }
        public decimal Price { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = (QuoteKey) obj;
            return Broker == that.Broker &&
                   Side == that.Side &&
                   Price == that.Price;
        }

        public override int GetHashCode()
        {
            return Broker.GetHashCode() ^ Side.GetHashCode() ^ Price.GetHashCode();
        }
    }
}