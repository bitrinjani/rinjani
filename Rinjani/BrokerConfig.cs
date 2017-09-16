namespace Rinjani
{
    public class BrokerConfig
    {
        public Broker Broker { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public bool Enabled { get; set; }
        public decimal MaxLongPosition { get; set; }
        public decimal MaxShortPosition { get; set; }
        public CashMarginType CashMarginType { get; set; }
        public int LeverageLevel { get; set; }
    }
}