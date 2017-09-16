namespace Rinjani.Bitflyer
{
    public class Balance
    {
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public decimal Available { get; set; }
    }
}