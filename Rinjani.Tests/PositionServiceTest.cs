using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rinjani.Tests
{
    [TestClass]
    public class PositionServiceTest
    {
        [TestMethod]
        public void TestPositionService()
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
            var mBARouter = new Mock<IBrokerAdapterRouter>();
            mBARouter.Setup(x => x.GetBtcPosition(Broker.Quoine)).Returns(0.2m);
            mBARouter.Setup(x => x.GetBtcPosition(Broker.Coincheck)).Returns(-0.3m);
            var baRouter = mBARouter.Object;
            var mTimer = new Mock<ITimer>();

            var ps = new PositionService(configStore, baRouter, mTimer.Object);
            var positions = ps.PositionMap.Values.ToList();
            var exposure = ps.NetExposure;
            var ccPos = positions.First(x => x.Broker == Broker.Coincheck);

            Assert.IsTrue(positions.Count == 2);
            Assert.AreEqual(-0.1m, exposure);
            Assert.AreEqual(positions.Sum(p => p.Btc), exposure);
            Assert.AreEqual(-0.3m, ccPos.Btc);
            Assert.IsTrue(ccPos.LongAllowed);
            Assert.IsTrue(!ccPos.ShortAllowed);
            Assert.AreEqual(1.3m, ccPos.AllowedLongSize);
            Assert.AreEqual(0, ccPos.AllowedShortSize);
        }
    }
}