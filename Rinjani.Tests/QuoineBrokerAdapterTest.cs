using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rinjani.Quoine;
using RestSharp.Deserializers;

namespace Rinjani.Tests
{
    [TestClass]
    public class QuoineBrokerAdapterTest
    {
        private Mock<IRestClient> _restClient;
        private Mock<IConfigStore> _configStore;

        [TestInitialize]
        public void Initialize()
        {
            _restClient = new Mock<IRestClient>();
            _restClient.SetupAllProperties();
            _configStore = new Mock<IConfigStore>();
            var config = new ConfigRoot
            {
                Brokers = new List<BrokerConfig>()
                {
                    new BrokerConfig
                    {
                        Broker = Broker.Quoine,
                        CashMarginType = CashMarginType.NetOut,
                        Key = "",
                        Secret = ""
                    }
                }
            };
            _configStore.Setup(x => x.Config).Returns(config);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            Assert.AreEqual(Broker.Quoine, ba.Broker);
            Assert.IsTrue(_restClient.Object.BaseUrl.ToString().Contains("quoine"));
        }

        [TestMethod]
        public void CashMarginTypeTest()
        {
            var allowedType = new[] { CashMarginType.MarginClose, CashMarginType.MarginOpen, CashMarginType.NetOut };
            foreach (CashMarginType cmType in Enum.GetValues(typeof(CashMarginType)))
            {
                if (!allowedType.Contains(cmType))
                {
                    try
                    {
                        _configStore.Object.Config.Brokers[0].CashMarginType = cmType;
                        var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
                        Assert.Fail();
                    }
                    catch (NotSupportedException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        [TestMethod]
        public void SendTest()
        {
            var content = @"{
                              ""id"": 2157474,
                              ""order_type"": ""limit"",
                              ""quantity"": ""0.01"",
                              ""disc_quantity"": ""0.0"",
                              ""iceberg_total_quantity"": ""0.0"",
                              ""side"": ""sell"",
                              ""filled_quantity"": ""0.0"",
                              ""price"": ""500.0"",
                              ""created_at"": 1462123639,
                              ""updated_at"": 1462123639,
                              ""status"": ""live"",
                              ""leverage_level"": 1,
                              ""source_exchange"": ""QUOINE"",
                              ""product_id"": 1,
                              ""product_code"": ""CASH"",
                              ""funding_currency"": ""JPY"",
                              ""currency_pair_code"": ""BTCJPY"",
                              ""order_fee"": ""0.0""
                            }";
            SetupRestMock<SendReply>(content);
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            var order = new Order { Broker = Broker.Quoine };
            ba.Send(order);
            Assert.AreEqual("2157474", order.BrokerOrderId);
            Assert.AreEqual(OrderStatus.New, order.Status);
            Assert.IsTrue((order.SentTime - DateTime.Now).Seconds < 10);
            Assert.IsTrue((order.LastUpdated - DateTime.Now).Seconds < 10);
        }

        [TestMethod]
        public void RefreshTest()
        {
            var content = @"{
                          ""id"": 2157479,
                          ""order_type"": ""limit"",
                          ""quantity"": ""0.01"",
                          ""disc_quantity"": ""0.0"",
                          ""iceberg_total_quantity"": ""0.0"",
                          ""side"": ""sell"",
                          ""filled_quantity"": ""0.01"",
                          ""price"": ""500.0"",
                          ""created_at"": 1462123639,
                          ""updated_at"": 1462123639,
                          ""status"": ""filled"",
                          ""leverage_level"": 2,
                          ""source_exchange"": ""QUOINE"",
                          ""product_id"": 1,
                          ""product_code"": ""CASH"",
                          ""funding_currency"": ""JPY"",
                          ""currency_pair_code"": ""BTCJPY"",
                          ""order_fee"": ""0.0"",
                          ""executions"": [
                            {
                              ""id"": 4566133,
                              ""quantity"": ""0.01"",
                              ""price"": ""500.0"",
                              ""taker_side"": ""buy"",
                              ""my_side"": ""sell"",
                              ""created_at"": 1465396785
                            }
                          ]
                        }";
            SetupRestMock<OrderStateReply>(content);
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            var order = new Order { Broker = Broker.Quoine, BrokerOrderId = "2157479", Size = 0.01m };
            ba.Refresh(order);
            Assert.AreEqual(0.01m, order.FilledSize);
            Assert.AreEqual(0m, order.PendingSize);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
            Assert.AreEqual(0.01m, order.Executions[0].Size);
            Assert.AreEqual(OrderSide.Buy, order.Executions[0].Side);
            Assert.IsTrue((order.LastUpdated - DateTime.Now).Seconds < 10);
        }

        [TestMethod]
        public void CancelTest()
        {
            var content = @"{
                              ""id"": 2157474,
                              ""order_type"": ""limit"",
                              ""quantity"": ""0.01"",
                              ""disc_quantity"": ""0.0"",
                              ""iceberg_total_quantity"": ""0.0"",
                              ""side"": ""sell"",
                              ""filled_quantity"": ""0.0"",
                              ""price"": ""500.0"",
                              ""created_at"": 1462123639,
                              ""updated_at"": 1462123639,
                              ""status"": ""cancelled"",
                              ""leverage_level"": 1,
                              ""source_exchange"": ""QUOINE"",
                              ""product_id"": 1,
                              ""product_code"": ""CASH"",
                              ""funding_currency"": ""JPY"",
                              ""currency_pair_code"": ""BTCJPY""
                            }";
            SetupRestMock(content);
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            var order = new Order { Broker = Broker.Quoine, BrokerOrderId = "2157479", Size = 0.01m };
            ba.Cancel(order);
            Assert.AreEqual(OrderStatus.Canceled, order.Status);
            Assert.IsTrue((order.LastUpdated - DateTime.Now).Seconds < 10);
        }

        [TestMethod]
        public void GetBtcPositionTest()
        {
            var content = @"[
                            {
                            ""id"": 1759,
                            ""leverage_level"": 10,
                            ""max_leverage_level"": 10,
                            ""pnl"": ""0.0"",
                            ""equity"": ""10000.1773"",
                            ""margin"": ""4.2302"",
                            ""free_margin"": ""9995.9471"",
                            ""trader_id"": 4807,
                            ""status"": ""active"",
                            ""product_code"": ""CASH"",
                            ""currency_pair_code"": ""BTCUSD"",
                            ""position"": ""0.8"",
                            ""balance"": ""50000.1773"",
                            ""created_at"": 1421992165,
                            ""updated_at"": 1457242996,
                            ""pusher_channel"": ""trading_account_1759"",
                            ""margin_percent"": ""0.1"",
                            ""product_id"": 1,
                            ""funding_currency"": ""USD""
                            },  
                            {
                            ""id"": 1759,
                            ""leverage_level"": 10,
                            ""max_leverage_level"": 10,
                            ""pnl"": ""0.0"",
                            ""equity"": ""10000.1773"",
                            ""margin"": ""4.2302"",
                            ""free_margin"": ""9995.9471"",
                            ""trader_id"": 4807,
                            ""status"": ""active"",
                            ""product_code"": ""CASH"",
                            ""currency_pair_code"": ""BTCJPY"",
                            ""position"": ""0.1"",
                            ""balance"": ""10000.1773"",
                            ""created_at"": 1421992165,
                            ""updated_at"": 1457242996,
                            ""pusher_channel"": ""trading_account_1759"",
                            ""margin_percent"": ""0.1"",
                            ""product_id"": 1,
                            ""funding_currency"": ""JPY""
                            }
                        ]";
            SetupRestMock<List<TradingAccounts>>(content);
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            var pos = ba.GetBtcPosition();
            Assert.AreEqual(0.1m, pos);
        }

        [TestMethod]
        public void FetchQuoteTest()
        {
            var content = @"{
                            ""buy_price_levels"": [
                            [""416.23000"", ""1.75000""]
                            ],
                            ""sell_price_levels"": [
                            [""416.47000"", ""0.28675""]
                            ]
                        }";
            SetupRestMock<Depth>(content);
            var ba = new BrokerAdapter(_restClient.Object, _configStore.Object);
            var quotes = ba.FetchQuotes();
            var ask = quotes.First(x => x.Side == QuoteSide.Ask);
            var bid = quotes.First(x => x.Side == QuoteSide.Bid);
            Assert.AreEqual(bid.Price, 416.23m);
            Assert.AreEqual(bid.Volume, 1.75m);
            Assert.AreEqual(ask.Price, 416.47m);
            Assert.AreEqual(ask.Volume, 0.28675m);
        }

        private void SetupRestMock<T>(string content) where T : new()
        {
            var restResponse = new RestResponse<T>
            {
                Content = content,
                StatusCode = System.Net.HttpStatusCode.OK,
            };
            restResponse.Data = new JsonDeserializer().Deserialize<T>(restResponse);
            _restClient.Setup(x => x.Execute<T>(It.IsAny<RestRequest>())).Returns(restResponse);
        }

        private void SetupRestMock(string content)
        {
            var restResponse = new RestResponse
            {
                Content = content,
                StatusCode = System.Net.HttpStatusCode.OK,
            };
            _restClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(restResponse);
        }
    }
}
