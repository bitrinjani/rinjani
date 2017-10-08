using Rinjani.Properties;
using System;
using System.Linq;

namespace Rinjani
{
    public class ConfigValidator : IConfigValidator
    {
        public void Validate(ConfigRoot config)
        {
            ThrowIf(config.Brokers.Count < 2, Resources.AtLeastTwoBrokersMustBeEnabled);
            MustBePositive(config.IterationInterval, nameof(config.IterationInterval));
            MustBePositive(config.MaxNetExposure, nameof(config.MaxNetExposure));
            MustBePositive(config.MaxRetryCount, nameof(config.MaxRetryCount));
            MustBeGreaterThanZero(config.MaxSize, nameof(config.MaxSize));
            MustBeGreaterThanZero(config.MinSize, nameof(config.MinSize));
            MustBePositive(config.MinTargetProfit, nameof(config.MinTargetProfit));
            MustBePositive(config.OrderStatusCheckInterval, nameof(config.OrderStatusCheckInterval));
            MustBePositive(config.PositionRefreshInterval, nameof(config.PositionRefreshInterval));
            MustBePositive(config.PriceMergeSize, nameof(config.PriceMergeSize));
            MustBePositive(config.QuoteRefreshInterval, nameof(config.QuoteRefreshInterval));
            MustBePositive(config.SleepAfterSend, nameof(config.SleepAfterSend));

            var bitflyer = config.Brokers.FirstOrDefault(b => b.Broker == Broker.Bitflyer);
            if (IsEnabled(bitflyer))
            {
                ThrowIf(bitflyer.CashMarginType != CashMarginType.Cash,
                    "CashMarginType must be Cash for Bitflyer.");
                ValidateBrokerConfigCommon(bitflyer);
            }

            var coincheck = config.Brokers.FirstOrDefault(b => b.Broker == Broker.Coincheck);
            if (IsEnabled(coincheck))
            {
                ThrowIf(coincheck.CashMarginType != CashMarginType.MarginOpen,
                    "CashMarginType must be MarginOpen for Coincheck.");
                ValidateBrokerConfigCommon(coincheck);
            }

            var quoine = config.Brokers.FirstOrDefault(b => b.Broker == Broker.Quoine);
            if (IsEnabled(quoine))
            {
                ThrowIf(quoine.CashMarginType != CashMarginType.NetOut,
                    "CashMarginType must be NetOut for Quoine.");
                ValidateBrokerConfigCommon(quoine);
            }
        }

        private void MustBePositive(int number, string name)
        {
            MustBePositive((decimal)number, name);
        }

        private void MustBePositive(decimal number, string name)
        {
            ThrowIf(number <= 0m, $"{name} must be positive.");
        }

        private void MustBeGreaterThanZero(int number, string name)
        {
            MustBeGreaterThanZero((decimal)number, name);
        }

        private void MustBeGreaterThanZero(decimal number, string name)
        {
            ThrowIf(number < 0m, $"{name} must be zero or positive.");
        }

        private void ValidateBrokerConfigCommon(BrokerConfig brokerConfig)
        {
            MustBeGreaterThanZero(brokerConfig.MaxLongPosition, nameof(brokerConfig.MaxLongPosition));
            MustBeGreaterThanZero(brokerConfig.MaxShortPosition, nameof(brokerConfig.MaxShortPosition));
        }

        private bool IsEnabled(BrokerConfig brokerConfig)
        {
            return brokerConfig != null && brokerConfig.Enabled;
        }

        private void ThrowIf(bool condition, string message)
        {
            if (condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}