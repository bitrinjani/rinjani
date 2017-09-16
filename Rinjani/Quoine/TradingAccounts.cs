namespace Rinjani.Quoine
{
    public class TradingAccounts
    {
        public int Id { get; set; }
        public int LeverageLevel { get; set; }
        public int MaxLeverageLevel { get; set; }
        public decimal Pnl { get; set; }
        public decimal Equity { get; set; }
        public decimal Margin { get; set; }
        public decimal FreeMargin { get; set; }
        public int TradersId { get; set; }
        public string Status { get; set; }
        public string ProductCode { get; set; }
        public string CurrencyPairCode { get; set; }
        public decimal Position { get; set; }
        public decimal Balance { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public string PusherChannel { get; set; }
        public decimal MarginPercent { get; set; }
        public int ProductId { get; set; }
        public string FundingCurrency { get; set; }
    }
}