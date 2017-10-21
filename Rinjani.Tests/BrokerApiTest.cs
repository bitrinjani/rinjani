using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;

namespace Rinjani.Tests
{
    [TestClass]
    [Ignore]
    [DeploymentItem(ConfigPath)]
    public class BrokerApiTest
    {
        private const string ConfigPath = "config.json";

        [TestMethod]
        //[Ignore]
        public void CoincheckTest()
        {
            var broker = Broker.Coincheck;
            SendRefreshCancelAssert(broker);
        }

        [TestMethod]
        //[Ignore]
        public void BitflyerTest()
        {
            var broker = Broker.Bitflyer;
            SendRefreshCancelAssert(broker);
        }

        [TestMethod]
        //[Ignore]
        public void QuoineTest()
        {
            var broker = Broker.Quoine;
            SendRefreshCancelAssert(broker);
        }


        private static void SendRefreshCancelAssert(Broker broker)
        {
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            var router = new BrokerAdapterRouter(new List<IBrokerAdapter>
            {
                new Bitflyer.BrokerAdapter(new RestClient(), configStore),
                new Coincheck.BrokerAdapter(new RestClient(), configStore),
                new Quoine.BrokerAdapter(new RestClient(), configStore)
            });
            var quotes = router.FetchQuotes(broker);
            var bestAsk = quotes.Min(x => x.Price);
            var targetAsk = bestAsk - 50000;
            var order = new Order
            {
                Broker = broker,
                Size = 0.01m,
                Price = targetAsk,
                Side = OrderSide.Buy,
                CashMarginType = brokerConfig.CashMarginType,
                LeverageLevel = brokerConfig.LeverageLevel
            };
            Debug.WriteLine(order);
            Assert.AreEqual(OrderStatus.PendingNew, order.Status);
            Assert.AreEqual(null, order.BrokerOrderId);

            router.Send(order);

            Debug.WriteLine("Sent the order.");
            Debug.WriteLine(order);
            Assert.AreEqual(OrderStatus.New, order.Status);
            Assert.IsTrue(order.BrokerOrderId != null);
            Assert.IsTrue(!order.BrokerOrderId.IsNullOrEmpty());

            var lastUpdated = order.LastUpdated;
            while (true)
            {
                Debug.WriteLine("Checking if order is refreshed.");
                Thread.Sleep(500);

                router.Refresh(order);

                Debug.WriteLine("Refreshed.");
                Debug.WriteLine(order);
                if (lastUpdated != order.LastUpdated)
                {
                    break;
                }
                Debug.WriteLine("Not refreshed yet.");
            }
            Assert.AreEqual(OrderStatus.New, order.Status);

            router.Cancel(order);

            Debug.WriteLine("Canceled.");
            Debug.WriteLine(order);
            Assert.AreEqual(OrderStatus.Canceled, order.Status);
        }

        [TestMethod]
        //[Ignore]
        public void CoincheckGetBtcPositionTest()
        {
            var broker = Broker.Coincheck;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            var dic = new Dictionary<CashMarginType, decimal>();
            foreach (CashMarginType cashMarginEnum in Enum.GetValues(typeof(CashMarginType)))
            {
                brokerConfig.CashMarginType = cashMarginEnum;
                var ba = new Coincheck.BrokerAdapter(new RestClient(), configStore);
                var btcPosition = ba.GetBtcPosition();
                Debug.WriteLine($"{broker} {cashMarginEnum}: {btcPosition}");
                dic.Add(cashMarginEnum, btcPosition);
            }
            Assert.AreEqual(dic[CashMarginType.MarginClose], dic[CashMarginType.MarginOpen]);
            Assert.AreEqual(dic[CashMarginType.NetOut], dic[CashMarginType.MarginOpen]);
            Assert.AreEqual(dic[CashMarginType.MarginClose], dic[CashMarginType.NetOut]);
        }

        [TestMethod]
        //[Ignore]
        public void BitflyerGetBtcPositionTest()
        {
            var broker = Broker.Bitflyer;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            brokerConfig.CashMarginType = CashMarginType.Cash;
            var ba = new Bitflyer.BrokerAdapter(new RestClient(), configStore);
            var btcPosition = ba.GetBtcPosition();
            Debug.WriteLine($"{broker} {brokerConfig.CashMarginType}: {btcPosition}");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        //[Ignore]
        public void BitflyerGetBtcPositionTest2()
        {
            var broker = Broker.Bitflyer;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            brokerConfig.CashMarginType = CashMarginType.MarginOpen;
            var ba = new Bitflyer.BrokerAdapter(new RestClient(), configStore);
            var btcPosition = ba.GetBtcPosition();
            Debug.WriteLine($"{broker} {brokerConfig.CashMarginType}: {btcPosition}");
        }

        [TestMethod]
        //[Ignore]
        public void QuoineGetBtcPositionTest()
        {
            var broker = Broker.Quoine;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            var dic = new Dictionary<CashMarginType, decimal>();
            foreach (CashMarginType cashMarginEnum in Enum.GetValues(typeof(CashMarginType)))
            {
                if (cashMarginEnum == CashMarginType.Cash)
                    continue;
                brokerConfig.CashMarginType = cashMarginEnum;
                var ba = new Quoine.BrokerAdapter(new RestClient(), configStore);
                var btcPosition = ba.GetBtcPosition();
                Debug.WriteLine($"{broker} {cashMarginEnum}: {btcPosition}");
                dic.Add(cashMarginEnum, btcPosition);
            }

            Assert.AreEqual(dic[CashMarginType.MarginClose], dic[CashMarginType.MarginOpen]);
            Assert.AreEqual(dic[CashMarginType.NetOut], dic[CashMarginType.MarginOpen]);
            Assert.AreEqual(dic[CashMarginType.MarginClose], dic[CashMarginType.NetOut]);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        //[Ignore]
        public void QuoineGetBtcPositionTest2()
        {
            var broker = Broker.Quoine;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            brokerConfig.CashMarginType = CashMarginType.Cash;
            var ba = new Quoine.BrokerAdapter(new RestClient(), configStore);
            var btcPosition = ba.GetBtcPosition();
            Debug.WriteLine($"{broker} {brokerConfig.CashMarginType}: {btcPosition}");
        }

        [TestMethod]
        //[Ignore]
        public void CoincheckGetBtcPositionTest2()
        {
            var broker = Broker.Coincheck;
            var configStore = new JsonConfigStore(ConfigPath, new List<IConfigValidator>());
            var brokerConfig = configStore.Config.Brokers.First(x => x.Broker == broker);
            var ba = new Coincheck.BrokerAdapter(new RestClient(), configStore);
            var btcPosition = ba.GetBtcPosition();
            Debug.WriteLine($"{broker}: {btcPosition}");
        }
    }
}