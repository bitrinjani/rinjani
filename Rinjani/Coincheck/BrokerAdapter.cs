using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using RestSharp;

namespace Rinjani.Coincheck
{
    public class BrokerAdapter : IBrokerAdapter
    {
        private const string ApiRoot = "https://coincheck.com";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly BrokerConfig _config;
        private readonly IRestClient _restClient;

        public BrokerAdapter(IRestClient restClient, IConfigStore configStore)
        {
            if (configStore == null)
            {
                throw new ArgumentNullException(nameof(configStore));
            }
            _config = configStore.Config.Brokers.First(b => b.Broker == Broker);
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _restClient.BaseUrl = new Uri(ApiRoot);
        }

        public Broker Broker => Broker.Coincheck;

        public decimal GetBtcPosition()
        {
            if (_config.CashMarginType == CashMarginType.Cash)
            {
                return GetBalance().Btc;
            }
            var positions = GetLeverageOpenPositions();
            var longPosition = positions.Where(p => p.Side == OrderSide.Buy).Sum(p => p.Size);
            var shortPosition = positions.Where(p => p.Side == OrderSide.Sell).Sum(p => p.Size);
            return longPosition - shortPosition;
        }

        public IList<Quote> FetchQuotes()
        {
            try
            {
                Log.Debug($"Getting depth from {Broker}...");
                var path = "/api/order_books";
                var req = RestUtil.CreateJsonRestRequest(path);
                var response = _restClient.Execute<Depth>(req);
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                Log.Debug($"Received depth from {Broker}.");
                var quotes = response.Data.ToQuotes();
                return quotes ?? new List<Quote>();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Debug(ex);
                return new List<Quote>();
            }
        }

        public void Send(Order order)
        {
            if (order.Broker != Broker)
            {
                throw new InvalidOperationException();
            }

            var orderType = GetBrokerOrderType(order);
            var method = "POST";
            var path = "/api/exchange/orders";
            var body = $"pair=btc_jpy&order_type={orderType}&amount={order.Size}";
            if (order.Type != OrderType.Market)
            {
                body += $"&rate={order.Price}";
            }

            if (order.CashMarginType == CashMarginType.MarginClose && order.ClosingMarginPositionId != null)
            {
                if (order.ClosingMarginPositionId == null)
                {
                    throw new InvalidOperationException("Margin close order requires the closing position ID.");
                }

                body += $"&position_id={order.ClosingMarginPositionId}";
            }

            var req = BuildRequest(path, method, body);
            var reply = RestUtil.ExecuteRequest<SendReply>(_restClient, req);
            if (!reply.Success)
            {
                throw new InvalidOperationException("Send failed.");
            }

            order.SentTime = Util.IsoDateTimeToLocal(reply.CreatedAt);
            order.Status = OrderStatus.New;
            order.BrokerOrderId = reply.Id;
            order.LastUpdated = DateTime.Now;
        }

        public void Cancel(Order order)
        {
            var orderId = order.BrokerOrderId;
            var reply = Cancel(orderId);
            if (!reply.Success)
            {
                throw new InvalidOperationException($"Cancel {orderId} failed.");
            }

            order.LastUpdated = DateTime.Now;
            order.Status = OrderStatus.Canceled;
        }

        public void Refresh(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (order.BrokerOrderId == null)
            {
                throw new InvalidOperationException("Broker order ID is not set.");
            }

            var reply = GetOpenOrders();
            var brokerOrder = reply.orders.FirstOrDefault(o => o.id == order.BrokerOrderId);
            if (brokerOrder != null)
            {
                if (brokerOrder.pending_amount == null || brokerOrder.pending_amount == 0)
                {
                    throw new InvalidOperationException("Unexpected reply returned.");
                }

                order.FilledSize = order.Size - (decimal)brokerOrder.pending_amount;
                if (order.FilledSize > 0)
                {
                    order.Status = OrderStatus.PartiallyFilled;
                }

                order.LastUpdated = DateTime.Now;
                return;
            }

            var searchAfter = order.CreationTime.AddMinutes(-1);
            var transactions = GetTransactions(searchAfter).Where(x => x.order_id == order.BrokerOrderId).ToList();
            if (transactions.Count == 0)
            {
                Log.Warn("The order is not found in pending orders and historical orders.");
                return;
            }

            order.Executions = transactions.Select(x => new Execution(order)
            {
                ExecTime = Util.IsoDateTimeToLocal(x.created_at),
                Price = x.rate,
                Size = Math.Abs(x.funds.btc)
            }).ToList();
            order.FilledSize = order.Executions.Sum(x => x.Size);
            order.Status = order.FilledSize == order.Size ? OrderStatus.Filled : OrderStatus.Canceled;
            order.LastUpdated = DateTime.Now;
        }

        private CancelReply Cancel(string orderId)
        {
            var method = "DELETE";
            var path = $"/api/exchange/orders/{orderId}";
            var req = BuildRequest(path, method);
            var reply = RestUtil.ExecuteRequest<CancelReply>(_restClient, req);
            return reply;
        }

        private List<TransactionsReply.Datum> GetTransactions(DateTime searchAfter)
        {
            var transactions = new List<TransactionsReply.Datum>();
            var transactionReply = GetTransactions();
            while (true)
            {
                transactions.AddRange(transactionReply.data.Where(x =>
                    searchAfter < Util.IsoDateTimeToLocal(x.created_at)));
                if (searchAfter > Util.IsoDateTimeToLocal(transactionReply.data.Last().created_at))
                {
                    break;
                }

                if (transactionReply.data.Count == 0 ||
                    transactionReply.pagination.limit > transactionReply.data.Count)
                {
                    break;
                }

                var lastId = transactionReply.data.Last().id;
                transactionReply = GetTransactions(lastId);
            }
            return transactions;
        }

        private OpenOrdersReply GetOpenOrders()
        {
            var path = "/api/exchange/orders/opens";
            var req = BuildRequest(path);
            return RestUtil.ExecuteRequest<OpenOrdersReply>(_restClient, req);
        }

        private TransactionsReply GetTransactions(string after = null, string before = null, int limit = 20,
            string order = "desc")
        {
            var path = $"/api/exchange/orders/transactions_pagination?limit={limit}&order={order}";
            if (after != null)
            {
                path += $"&starting_after={after}";
            }

            if (before != null)
            {
                path += $"&ending_before={before}";
            }

            var req = BuildRequest(path);
            return RestUtil.ExecuteRequest<TransactionsReply>(_restClient, req);
        }

        private static BrokerOrderType GetBrokerOrderType(Order order)
        {
            switch (order.CashMarginType)
            {
                case CashMarginType.Cash:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            switch (order.Type)
                            {
                                case OrderType.Market:
                                    return BrokerOrderType.market_buy;
                                case OrderType.Limit:
                                    return BrokerOrderType.buy;
                                default:
                                    throw new NotImplementedException();
                            }

                        case OrderSide.Sell:
                            switch (order.Type)
                            {
                                case OrderType.Market:
                                    return BrokerOrderType.market_sell;
                                case OrderType.Limit:
                                    return BrokerOrderType.sell;
                                default:
                                    throw new NotImplementedException();
                            }
                        default:
                            throw new NotImplementedException();
                    }
                case CashMarginType.MarginOpen:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            return BrokerOrderType.leverage_buy;
                        case OrderSide.Sell:
                            return BrokerOrderType.leverage_sell;
                        default:
                            throw new NotImplementedException();
                    }

                case CashMarginType.MarginClose:
                    switch (order.Side)
                    {
                        case OrderSide.Buy:
                            return BrokerOrderType.close_short;
                        case OrderSide.Sell:
                            return BrokerOrderType.close_long;
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private IList<LeveragePosition> GetLeverageOpenPositions()
        {
            var path = "/api/exchange/leverage/positions?status=open&limit=25";
            var req = BuildRequest(path);
            var reply = RestUtil.ExecuteRequest<LeveragePositionReply>(_restClient, req);
            if (reply.data.Count > 25)
            {
                throw new InvalidOperationException("Paging handling not implemented.");
            }
            var leveragePositions = reply.data.Select(record => new LeveragePosition
            {
                Id = record.id,
                Side = Util.ParseEnum<OrderSide>(record.side),
                Size = record.amount
            }).ToList();
            return leveragePositions;
        }

        private BalanceReply GetBalance()
        {
            var path = "/api/accounts/balance";
            var req = BuildRequest(path);
            return RestUtil.ExecuteRequest<BalanceReply>(_restClient, req);
        }

        private RestRequest BuildRequest(string path, string method = "GET", string body = "")
        {
            var nonce = Util.Nonce;
            var url = ApiRoot + path;
            var message = nonce + url + body;
            var sign = Util.GenerateNewHmac(_config.Secret, message);
            var req = RestUtil.CreateJsonRestRequest(path, false);
            req.Method = Util.ParseEnum<Method>(method);
            if (body != "")
            {
                req.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
            }
            req.AddHeader("ACCESS-KEY", _config.Key);
            req.AddHeader("ACCESS-NONCE", nonce);
            req.AddHeader("ACCESS-SIGNATURE", sign);
            return req;
        }
    }
}
