using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;

namespace Rinjani
{
    public class QuoteAggregator : IQuoteAggregator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IList<IBrokerAdapter> _brokerAdapters;
        private readonly ConfigRoot _config;
        private readonly ITimer _timer;
        private bool _isRunning;
        private IList<Quote> _quotes;

        public QuoteAggregator(IConfigStore configStore, IList<IBrokerAdapter> brokerAdapters, ITimer timer)
        {
            _config = configStore?.Config ?? throw new ArgumentNullException(nameof(configStore));
            _brokerAdapters = brokerAdapters ?? throw new ArgumentNullException(nameof(brokerAdapters));
            _timer = timer;
            Util.StartTimer(timer, _config.QuoteRefreshInterval, OnTimerTriggered);
            Aggregate();
        }

        public event EventHandler QuoteUpdated;

        public IList<Quote> Quotes
        {
            get => _quotes;
            private set
            {
                _quotes = value;
                OnQuoteUpdated();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void OnTimerTriggered(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_isRunning)
            {
                return;
            }
            try
            {
                _isRunning = true;
                Aggregate();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Debug(ex);
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void Aggregate()
        {
            Log.Debug("Aggregating quotes...");
            Quotes = _brokerAdapters.SelectMany(x => x.FetchQuotes().ToList()).Merge(_config.PriceMergeSize).ToList();
            Log.Debug("Aggregated.");
        }

        protected virtual void OnQuoteUpdated()
        {
            QuoteUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}