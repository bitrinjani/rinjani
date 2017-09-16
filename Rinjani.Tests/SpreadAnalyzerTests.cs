using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rinjani.Tests
{
    [TestClass]
    public class SpreadAnalyzerTests
    {
        [TestMethod]
        public void AnalyzeTest()
        {
            var mConfigRepository = new Mock<IConfigStore>();
            var mPositionService = new Mock<IPositionService>();
            var config = new ConfigRoot {MaxSize = 0.5m};
            mConfigRepository.Setup(x => x.Config).Returns(config);
            mPositionService.Setup(x => x.PositionMap).Returns(new Dictionary<Broker, BrokerPosition>
            {
                {
                    Broker.Coincheck,
                    new BrokerPosition
                    {
                        AllowedLongSize = 10,
                        AllowedShortSize = 10,
                        LongAllowed = true,
                        ShortAllowed = true
                    }
                },
                {
                    Broker.Quoine,
                    new BrokerPosition
                    {
                        AllowedLongSize = 10,
                        AllowedShortSize = 10,
                        LongAllowed = true,
                        ShortAllowed = true
                    }
                }
            });

            var configStore = mConfigRepository.Object;
            var positionService = mPositionService.Object;

            var quotes = new List<Quote>
            {
                new Quote(Broker.Coincheck, QuoteSide.Ask, 3, 1),
                new Quote(Broker.Coincheck, QuoteSide.Bid, 2, 2),
                new Quote(Broker.Quoine, QuoteSide.Ask, 3.5m, 3),
                new Quote(Broker.Quoine, QuoteSide.Bid, 2.5m, 4)
            };

            var target = new SpreadAnalyzer(configStore, positionService);
            var result = target.Analyze(quotes);
            Assert.AreEqual(Broker.Coincheck, result.BestAsk.Broker);
            Assert.AreEqual(3, result.BestAsk.Price);
            Assert.AreEqual(1, result.BestAsk.Volume);
            Assert.AreEqual(Broker.Quoine, result.BestBid.Broker);
            Assert.AreEqual(2.5m, result.BestBid.Price);
            Assert.AreEqual(4, result.BestBid.Volume);
            Assert.AreEqual(-0.5m, result.InvertedSpread);
            Assert.AreEqual(0.5m, result.TargetVolume);
        }
    }
}