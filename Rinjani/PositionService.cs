using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;

namespace Rinjani
{
    public class PositionService : IPositionService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IBrokerAdapterRouter _brokerAdapterRouter;
        private readonly IConfigStore _configStore;
        private readonly ITimer _timer;
        private bool _isRefreshing;

        public PositionService(IConfigStore configStore, IBrokerAdapterRouter brokerAdapterRouter,
            ITimer timer)
        {
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _brokerAdapterRouter = brokerAdapterRouter ?? throw new ArgumentNullException(nameof(brokerAdapterRouter));
            _timer = timer;
            Util.StartTimer(timer, _configStore.Config.PositionRefreshInterval, OnTimerTriggered);
            Refresh();
        }

        public decimal NetExposure => PositionMap.Values.Sum(p => p.Btc);

        public IDictionary<Broker, BrokerPosition> PositionMap { get; private set; } =
            new Dictionary<Broker, BrokerPosition>();

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void OnTimerTriggered(object sender, ElapsedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_isRefreshing)
            {
                return;
            }

            try
            {
                _isRefreshing = true;
                var config = _configStore.Config;
                var positionMap = new Dictionary<Broker, BrokerPosition>();
                foreach (var brokerConfig in config.Brokers.Where(b => b.Enabled))
                {
                    var currentBtc = GetPosition(brokerConfig.Broker);
                    var allowedLongSize = Math.Max(0, brokerConfig.MaxLongPosition - currentBtc);
                    var allowedShortSize = Math.Max(0, currentBtc + brokerConfig.MaxShortPosition);
                    var pos = new BrokerPosition
                    {
                        Broker = brokerConfig.Broker,
                        Btc = currentBtc,
                        LongAllowed = allowedLongSize > 0,
                        ShortAllowed = allowedShortSize > 0,
                        AllowedLongSize = allowedLongSize,
                        AllowedShortSize = allowedShortSize
                    };
                    positionMap.Add(brokerConfig.Broker, pos);
                }

                PositionMap = positionMap;
                LogPositions();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void LogPositions()
        {
            Log.Info(Util.Hr(21) + "POSITION" + Util.Hr(21));
            Log.Info($"Net Exposure: {NetExposure,5:0.00}");
            foreach (var position in PositionMap)
            {
                Log.Info(position.Value);
            }
            Log.Info(Util.Hr(50));
        }

        private decimal GetPosition(Broker broker)
        {
            return _brokerAdapterRouter.GetBtcPosition(broker);
        }
    }
}