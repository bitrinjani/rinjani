using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rinjani.Tests
{
    [TestClass]
    public class QuoteAggregatorTest
    {
        [TestMethod]
        public void TestQuoteAggregator()
        {
            var config = new ConfigRoot
            {
                Brokers = new List<BrokerConfig>
                {
                    new BrokerConfig
                    {
                        Broker = Broker.Quoine,
                        Enabled = true,
                        MaxLongPosition = 0.3m,
                        MaxShortPosition = 0.3m
                    },
                    new BrokerConfig
                    {
                        Broker = Broker.Coincheck,
                        Enabled = true,
                        MaxLongPosition = 1m,
                        MaxShortPosition = 0m
                    }
                }
            };
            var mConfigRepo = new Mock<IConfigStore>();
            mConfigRepo.Setup(x => x.Config).Returns(config);
            var configStore = mConfigRepo.Object;

            var mProxy1 = new Mock<IBrokerAdapter>();
            var quotes1 = new List<Quote>
            {
                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500000, 0.1m),
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490000, 0.2m)
            };
            mProxy1.Setup(x => x.FetchQuotes()).Returns(quotes1);
            var mProxy2 = new Mock<IBrokerAdapter>();
            var quotes2 = new List<Quote>
            {
                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500001, 0.01m),
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490001, 0.02m)
            };
            mProxy2.Setup(x => x.FetchQuotes()).Returns(quotes2);
            var baList = new List<IBrokerAdapter> {mProxy1.Object, mProxy2.Object};
            var mTimer = new Mock<ITimer>();
            var aggregator = new QuoteAggregator(configStore, baList, mTimer.Object);
            var quotes = aggregator.Quotes;
            Assert.AreEqual(3, quotes.Count);
        }
    }
}