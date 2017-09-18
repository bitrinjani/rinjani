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
                    new BrokerConfig()
                    {
                        Broker = Broker.Bitflyer,
                        Enabled = true
                    },
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

            var mBitflyerBa = new Mock<IBrokerAdapter>();
            mBitflyerBa.Setup(x => x.Broker).Returns(Broker.Bitflyer);
            var quotes1 = new List<Quote>
            {
                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500000, 0.1m),

                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500001, 0.01m)
            };
            mBitflyerBa.Setup(x => x.FetchQuotes()).Returns(quotes1);

            var mCoincheckBa = new Mock<IBrokerAdapter>();
            mCoincheckBa.Setup(x => x.Broker).Returns(Broker.Coincheck);
            var quotes2 = new List<Quote>
            {
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490001, 0.02m),
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490000, 0.2m)
            };
            mCoincheckBa.Setup(x => x.FetchQuotes()).Returns(quotes2);

            var mQuoineBa = new Mock<IBrokerAdapter>();
            mQuoineBa.Setup(x => x.Broker).Returns(Broker.Quoine);
            mQuoineBa.Setup(x => x.FetchQuotes()).Returns(new List<Quote>());

            var baList = new List<IBrokerAdapter> {mBitflyerBa.Object, mCoincheckBa.Object, mQuoineBa.Object};
            var mTimer = new Mock<ITimer>();
            var aggregator = new QuoteAggregator(configStore, baList, mTimer.Object);
            var quotes = aggregator.Quotes;
            Assert.AreEqual(3, quotes.Count);
        }

        [TestMethod]
        public void TestQuoteAggregatorWithDisabledBa()
        {
            var config = new ConfigRoot
            {
                Brokers = new List<BrokerConfig>
                {
                    new BrokerConfig()
                    {
                        Broker = Broker.Bitflyer,
                        Enabled = false
                    },
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

            var mBitflyerBa = new Mock<IBrokerAdapter>();
            mBitflyerBa.Setup(x => x.Broker).Returns(Broker.Bitflyer);
            var quotes1 = new List<Quote>
            {
                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500000, 0.1m),

                new Quote(Broker.Bitflyer, QuoteSide.Ask, 500001, 0.01m)
            };
            mBitflyerBa.Setup(x => x.FetchQuotes()).Returns(quotes1);

            var mCoincheckBa = new Mock<IBrokerAdapter>();
            mCoincheckBa.Setup(x => x.Broker).Returns(Broker.Coincheck);
            var quotes2 = new List<Quote>
            {
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490001, 0.02m),
                new Quote(Broker.Coincheck, QuoteSide.Bid, 490000, 0.2m)
            };
            mCoincheckBa.Setup(x => x.FetchQuotes()).Returns(quotes2);

            var mQuoineBa = new Mock<IBrokerAdapter>();
            mQuoineBa.Setup(x => x.Broker).Returns(Broker.Quoine);
            mQuoineBa.Setup(x => x.FetchQuotes()).Returns(new List<Quote>());

            var baList = new List<IBrokerAdapter> { mBitflyerBa.Object, mCoincheckBa.Object, mQuoineBa.Object };
            var mTimer = new Mock<ITimer>();
            var aggregator = new QuoteAggregator(configStore, baList, mTimer.Object);
            var quotes = aggregator.Quotes;
            Assert.AreEqual(1, quotes.Count);
        }
    }
}