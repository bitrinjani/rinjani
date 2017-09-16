using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using static System.Threading.Thread;

namespace Rinjani
{
    public class Arbitrager : IArbitrager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly List<Order> _activeOrders = new List<Order>();
        private readonly IBrokerAdapterRouter _brokerAdapterRouter;
        private readonly IConfigStore _configStore;
        private readonly IPositionService _positionService;
        private readonly IQuoteAggregator _quoteAggregator;
        private readonly ISpreadAnalyzer _spreadAnalyzer;

        public Arbitrager(IQuoteAggregator quoteAggregator,
            IConfigStore configStore,
            IPositionService positionService,
            IBrokerAdapterRouter brokerAdapterRouter,
            ISpreadAnalyzer spreadAnalyzer)
        {
            _quoteAggregator = quoteAggregator ?? throw new ArgumentNullException(nameof(quoteAggregator));
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _brokerAdapterRouter = brokerAdapterRouter ?? throw new ArgumentNullException(nameof(brokerAdapterRouter));
            _spreadAnalyzer = spreadAnalyzer ?? throw new ArgumentNullException(nameof(spreadAnalyzer));
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        }

        public void Start()
        {
            Log.Info($"Starting {nameof(Arbitrager)}...");
            _quoteAggregator.QuoteUpdated += QuoteUpdated;
            Log.Info($"Started {nameof(Arbitrager)}.");
        }

        public void Dispose()
        {
            _positionService?.Dispose();
            _quoteAggregator?.Dispose();
        }

        private void Arbitrage()
        {
            Log.Info("Looking for opportunity...");
            var config = _configStore.Config;
            CheckMaxNetExposure();
            SpreadAnalysisResult result;
            try
            {
                result = _spreadAnalyzer.Analyze(_quoteAggregator.Quotes);
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to get a spread analysis result. {ex.Message}");
                return;
            }

            var bestBid = result.BestBid;
            var bestAsk = result.BestAsk;
            var invertedSpread = result.InvertedSpread;
            var availableVolume = result.AvailableVolume;
            var targetVolume = result.TargetVolume;
            var targetProfit = result.TargetProfit;

            Log.Info("{0,-17}: {1}", "Best ask", bestAsk);
            Log.Info("{0,-17}: {1}", "Best bid", bestBid);
            Log.Info("{0,-17}: {1}", "Spread", -invertedSpread);
            Log.Info("{0,-17}: {1}", "Available volume", availableVolume);
            Log.Info("{0,-17}: {1}", "Target volume", targetVolume);
            Log.Info("{0,-17}: {1}", "Expected profit", targetProfit);

            if (invertedSpread <= 0)
            {
                Log.Info("No arbitrage opportunity. Spread is not inverted.");
                return;
            }

            Log.Info($"Found inverted quotes.");
            if (availableVolume < config.MinSize)
            {
                Log.Info($"Available volume is smaller than min size. ");
                return;
            }

            if (targetProfit < config.MinTargetProfit)
            {
                Log.Info($"Target profit is smaller than min profit.");
                return;
            }

            if (bestBid.Broker == bestAsk.Broker)
            {
                Log.Warn($"Ignoring intra-broker cross.");
                return;
            }

            if (config.DemoMode)
            {
                Log.Info(">>This is Demo mode. Not sending orders.");
                return;
            }

            Log.Info($">>Found arbitrage oppotunity.");
            Log.Info($">>Sending order targetting quote {bestAsk}...");
            SendOrder(bestAsk, targetVolume, OrderType.Limit);
            Log.Info($">>Sending order targetting quote {bestBid}...");
            SendOrder(bestBid, targetVolume, OrderType.Limit);
            CheckOrderState();
            Log.Info($"Sleeping {config.SleepAfterSend} after send.");
            _activeOrders.Clear();
            Sleep(config.SleepAfterSend);
        }

        private void CheckMaxNetExposure()
        {
            if (Math.Abs(_positionService.NetExposure) > _configStore.Config.MaxNetExposure)
            {
                var message = "Net exposure is larger than max net exposure.";
                throw new InvalidOperationException(message);
            }
        }

        private void CheckOrderState()
        {
            var buyOrder = _activeOrders.First(x => x.Side == OrderSide.Buy);
            var sellOrder = _activeOrders.First(x => x.Side == OrderSide.Sell);
            var config = _configStore.Config;
            foreach (var i in Enumerable.Range(1, config.MaxRetryCount))
            {
                Sleep(config.OrderStatusCheckInterval);
                Log.Info($">>Order check attempt {i}.");
                Log.Info(">>Checking if both legs are done or not...");

                try
                {
                    _brokerAdapterRouter.Refresh(buyOrder);
                    _brokerAdapterRouter.Refresh(sellOrder);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex.Message);
                    Log.Debug(ex);
                }

                if (buyOrder.Status != OrderStatus.Filled)
                {
                    Log.Warn($"Buy leg is not filled yet. Pending size is {buyOrder.PendingSize}.");
                }
                if (sellOrder.Status != OrderStatus.Filled)
                {
                    Log.Warn($"Sell leg is not filled yet. Pending size is {sellOrder.PendingSize}.");
                }

                if (buyOrder.Status == OrderStatus.Filled && sellOrder.Status == OrderStatus.Filled)
                {
                    var profit = Math.Round(sellOrder.FilledSize * sellOrder.AverageFilledPrice -
                                 buyOrder.FilledSize * buyOrder.AverageFilledPrice);
                    Log.Info(">>Both legs are successfully filled.");
                    Log.Info($">>Buy filled price is {buyOrder.AverageFilledPrice}.");
                    Log.Info($">>Sell filled price is {sellOrder.AverageFilledPrice}.");
                    Log.Info($">>Profit is {profit}.");
                    break;
                }

                if (i == config.MaxRetryCount)
                {
                    Log.Warn("Max retry count reached. Cancelling the pending orders.");
                    if (buyOrder.Status != OrderStatus.Filled)
                    {
                        _brokerAdapterRouter.Cancel(buyOrder);
                    }

                    if (sellOrder.Status != OrderStatus.Filled)
                    {
                        _brokerAdapterRouter.Cancel(sellOrder);
                    }
                    break;
                }
            }
        }

        private void QuoteUpdated(object sender, EventArgs e)
        {
            try
            {
                Log.Info(Util.Hr(20) + "ARBITRAGER" + Util.Hr(20));
                Arbitrage();
                Log.Info(Util.Hr(50));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Debug(ex);
                Environment.Exit(-1);
            }
        }

        private void SendOrder(Quote quote, decimal targetVolume, OrderType orderType)
        {
            var brokerConfig = _configStore.Config.Brokers.First(x => x.Broker == quote.Broker);
            var orderSide = quote.Side == QuoteSide.Ask ? OrderSide.Buy : OrderSide.Sell;
            var cashMarginType = brokerConfig.CashMarginType;
            var leverageLevel = brokerConfig.LeverageLevel;
            var order = new Order(quote.Broker, orderSide, targetVolume, quote.Price, cashMarginType, orderType,
                leverageLevel);
            _brokerAdapterRouter.Send(order);
            _activeOrders.Add(order);
        }
    }
}