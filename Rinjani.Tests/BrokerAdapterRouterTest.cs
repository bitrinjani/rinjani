using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;

namespace Rinjani.Tests
{
    [TestClass]
    public class BrokerAdapterRouterTest
    {
        private List<IBrokerAdapter> _brokerAdapters;
        private Mock<IBrokerAdapter> _baBitflyer;
        private Mock<IBrokerAdapter> _baCoincheck;
        private Mock<IBrokerAdapter> _baQuoine;
        private BrokerAdapterRouter _target;

        [TestInitialize]
        public void Init()
        {
            _baBitflyer = new Mock<IBrokerAdapter>();
            _baBitflyer.Setup(x => x.Broker).Returns(Broker.Bitflyer);
            _baBitflyer.Setup(x => x.Send(It.IsAny<Order>()));

            _baCoincheck = new Mock<IBrokerAdapter>();
            _baCoincheck.Setup(x => x.Broker).Returns(Broker.Coincheck);
            _baCoincheck.Setup(x => x.Send(It.IsAny<Order>()));

            _baQuoine = new Mock<IBrokerAdapter>();
            _baQuoine.Setup(x => x.Broker).Returns(Broker.Quoine);
            _baQuoine.Setup(x => x.Send(It.IsAny<Order>()));

            _brokerAdapters = new List<IBrokerAdapter>() { _baBitflyer.Object, _baCoincheck.Object, _baQuoine.Object };
            _target = new BrokerAdapterRouter(_brokerAdapters);
        }

        [TestMethod]
        public void SendTest()
        {            
            var order = new Order(Broker.Bitflyer, OrderSide.Buy, 0.001m, 500000, CashMarginType.Cash, OrderType.Limit, 0);
            _target.Send(order);
            _baBitflyer.Verify(x => x.Send(order));         
        }

        [TestMethod]
        public void CancelTest()
        {
            var order = new Order(Broker.Coincheck, OrderSide.Buy, 0.001m, 500000, CashMarginType.Cash, OrderType.Limit, 0);
            _target.Cancel(order);
            _baCoincheck.Verify(x => x.Cancel(order));
        }

        [TestMethod]
        public void FetchQuoteTest()
        {
            _target.FetchQuotes(Broker.Quoine);
            _baQuoine.Verify(x => x.FetchQuotes());
        }

        [TestMethod]
        public void GetBtcPositionTest()
        {
            _target.GetBtcPosition(Broker.Quoine);
            _baQuoine.Verify(x => x.GetBtcPosition());
        }

        [TestMethod]
        public void GetOrderStateTest()
        {
            var order = new Order(Broker.Quoine, OrderSide.Buy, 0.001m, 500000, CashMarginType.Cash, OrderType.Limit, 0);
            _target.Refresh(order);
            _baQuoine.Verify(x => x.Refresh(order));
        }
    }
}
