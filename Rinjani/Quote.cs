namespace Rinjani
{
    public class Quote
    {
        public Quote(Broker broker, QuoteSide side, decimal price, decimal volume)
        {
            Broker = broker;
            Side = side;
            Price = price;
            Volume = volume;
        }

        public Quote()
        {
        }

        public Broker Broker { get; set; }
        public QuoteSide Side { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }

        public override string ToString()
        {
            return $"{Broker,10} {Side,5} {Price,7} {Volume:0.000}";
        }
    }
}