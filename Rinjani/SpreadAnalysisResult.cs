namespace Rinjani
{
    public class SpreadAnalysisResult
    {
        public Quote BestBid { get; set; }
        public Quote BestAsk { get; set; }
        public decimal InvertedSpread { get; set; }
        public decimal AvailableVolume { get; set; }
        public decimal TargetVolume { get; set; }
        public decimal TargetProfit { get; set; }
    }
}