namespace Rinjani
{
    public class BrokerPosition
    {
        public Broker Broker { get; set; }
        public bool LongAllowed { get; set; }
        public bool ShortAllowed { get; set; }
        public decimal Btc { get; set; }
        public decimal AllowedLongSize { get; set; }
        public decimal AllowedShortSize { get; set; }

        public override string ToString()
        {
            return
                $"{Broker.ToString(),10}: {Btc,5:0.00} BTC, LongAllowed: {LongAllowed,5}, ShortAllowed: {ShortAllowed,5}";
        }
    }
}