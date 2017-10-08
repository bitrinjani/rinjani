using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Rinjani.Properties;

namespace Rinjani
{
    public class SpreadAnalyzer : ISpreadAnalyzer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IConfigStore _configStore;
        private readonly IPositionService _positionService;

        public SpreadAnalyzer(IConfigStore configStore,
            IPositionService positionService)
        {
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        }

        public SpreadAnalysisResult Analyze(IList<Quote> quotes)
        {
            var config = _configStore.Config;
            var positionMap = _positionService.PositionMap;
            if (positionMap.Count == 0)
            {
                throw new InvalidOperationException("Position map is empty.");
            }

            var bestBid = quotes.Where(q => q.Side == QuoteSide.Bid)
                .Where(q => IsAllowed(q, positionMap[q.Broker]))
                .OrderByDescending(q => q.Price).FirstOrDefault();
            var bestAsk = quotes.Where(q => q.Side == QuoteSide.Ask)
                .Where(q => IsAllowed(q, positionMap[q.Broker]))
                .OrderBy(q => q.Price).FirstOrDefault();
            if (bestBid == null)
            {
                throw new InvalidOperationException(Resources.NoBestBidWasFound);
            }
            else if (bestAsk == null)
            {
                throw new InvalidOperationException(Resources.NoBestAskWasFound);
            }

            var invertedSpread = bestBid.Price - bestAsk.Price;
            var availableVolume = Util.RoundDown(Math.Min(bestBid.Volume, bestAsk.Volume), 2);
            var allowedShortSize = positionMap[bestBid.Broker].AllowedShortSize;
            var allowedLongSize = positionMap[bestAsk.Broker].AllowedLongSize;
            var targetVolume = new[] {availableVolume, config.MaxSize, allowedShortSize, allowedLongSize}.Min();
            targetVolume = Util.RoundDown(targetVolume, 2);
            var targetProfit = Math.Round(invertedSpread * targetVolume);

            return new SpreadAnalysisResult
            {
                BestBid = bestBid,
                BestAsk = bestAsk,
                InvertedSpread = invertedSpread,
                AvailableVolume = availableVolume,
                TargetVolume = targetVolume,
                TargetProfit = targetProfit
            };
        }

        private static bool IsAllowed(Quote q, BrokerPosition pos)
        {
            return q.Side == QuoteSide.Bid ? pos.ShortAllowed : pos.LongAllowed;
        }
    }
}