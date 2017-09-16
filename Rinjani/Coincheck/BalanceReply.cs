namespace Rinjani.Coincheck
{
    public class BalanceReply
    {
        public bool Success { get; set; }
        public decimal Jpy { get; set; }
        public decimal Btc { get; set; }
        public decimal JpyReserved { get; set; }
        public decimal BtcReserved { get; set; }
        public decimal JpyLendInUse { get; set; }
        public decimal BtcLendInUse { get; set; }
        public decimal JpyLent { get; set; }
        public decimal BtcLent { get; set; }
        public decimal JpyDebt { get; set; }
        public decimal BtcDebt { get; set; }
    }
}