using System.Collections.Generic;

namespace Rinjani
{
    public class ConfigRoot
    {
        public bool DemoMode { get; set; }
        public int PriceMergeSize { get; set; } = 10;
        public decimal MaxSize { get; set; }
        public decimal MinSize { get; set; }
        public decimal MinTargetProfit { get; set; }
        public int IterationInterval { get; set; } = 2000;
        public int PositionRefreshInterval { get; set; } = 10000;
        public int SleepAfterSend { get; set; } = 10000;
        public int QuoteRefreshInterval { get; set; } = 2000;
        public List<BrokerConfig> Brokers { get; set; }
        public decimal MaxNetExposure { get; set; } = 0.1m;
        public int MaxRetryCount { get; set; } = 30;
        public int OrderStatusCheckInterval { get; set; } = 3000;
    }
}