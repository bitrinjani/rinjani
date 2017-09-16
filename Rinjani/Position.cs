namespace Rinjani
{
    public class Position
    {
        public Broker Broker { get; set; }
        public string Symbol { get; set; }
        public OrderSide Side { get; set; }
        public bool IsMargin { get; set; }
    }
}