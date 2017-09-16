using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace Rinjani.Bitflyer
{
    public class BrokerAdapter : IBrokerAdapter
    {
        private const string ApiRoot = "https://api.bitflyer.jp";
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

        public Broker Broker => Broker.Bitflyer;

        public void Send(Order order)
        {
            if (order.Broker != Broker)
            {
                throw new InvalidOperationException();
            }

            var param = new SendChildOrderParam(order);
            var reply = Send(param);
            order.BrokerOrderId = reply.ChildOrderAcceptanceId;
            order.Status = OrderStatus.New;
            order.SentTime = DateTime.Now;
            order.LastUpdated = DateTime.Now;
        }

        public void Refresh(Order order)
        {
            var orderId = order.BrokerOrderId;
            var reply = GetChildOrders(orderId);
            var btOrderState = reply.FirstOrDefault();
            if (btOrderState == null)
            {
                var message = $"Unabled to find {orderId}. GetOrderState failed.";
                Log.Warn(message);
                return;
            }

            btOrderState.PopulateOrderProperties(order);
            var executionsReply = GetExecutions(orderId);
            order.Executions = executionsReply.Select(x => new Execution(order)
            {
                Size = x.size,
                Price = x.price,
                ExecTime = DateTime.Parse(x.exec_date)
            }).ToList();

            order.LastUpdated = DateTime.Now;
        }

        public void Cancel(Order order)
        {
            string productCode;
            switch (order.Symbol)
            {
                case "BTCJPY":
                    productCode = "BTC_JPY";
                    break;
                default:
                    throw new NotImplementedException();
            }
            Cancel(productCode, order.BrokerOrderId);
            order.LastUpdated = DateTime.Now;
            order.Status = OrderStatus.Canceled;
        }

        public decimal GetBtcPosition()
        {
            var path = "/v1/me/getbalance";
            var req = BuildRequest(path);
            var balanceList = RestUtil.ExecuteRequest<List<Balance>>(_restClient, req);
            var btcBalance = balanceList.Find(b => b.CurrencyCode == "BTC");
            return btcBalance.Amount;
        }

        public IList<Quote> FetchQuotes()
        {
            try
            {
                Log.Debug($"Getting depth from {_config.Broker}...");
                var path = "/v1/board";
                var req = RestUtil.CreateJsonRestRequest(path);
                var response = _restClient.Execute<Depth>(req);
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                Log.Debug($"Received depth from {_config.Broker}.");
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

        private SendReply Send(SendChildOrderParam param)
        {
            var method = "POST";
            var path = "/v1/me/sendchildorder";
            var body = JsonConvert.SerializeObject(param);
            var req = BuildRequest(path, method, body);
            return RestUtil.ExecuteRequest<SendReply>(_restClient, req);
        }

        private void Cancel(string productCode, string acceptanceId)
        {
            var method = "POST";
            var path = "/v1/me/cancelchildorder";
            var param = new CancelChildOrderParam
            {
                product_code = productCode,
                child_order_acceptance_id = acceptanceId
            };
            var body = JsonConvert.SerializeObject(param);
            var req = BuildRequest(path, method, body);
            RestUtil.ExecuteRequest(_restClient, req);
        }

        private List<ChildOrdersReply> GetChildOrders(string acceptanceId)
        {
            var path = $"/v1/me/getchildorders?child_order_acceptance_id={acceptanceId}";
            var req = BuildRequest(path);
            return RestUtil.ExecuteRequest<List<ChildOrdersReply>>(_restClient, req);
        }


        private List<ExecutionReply> GetExecutions(string acceptanceId)
        {
            var path = $"/v1/me/getexecutions?child_order_acceptance_id={acceptanceId}";
            var req = BuildRequest(path);
            return RestUtil.ExecuteRequest<List<ExecutionReply>>(_restClient, req);
        }

        private RestRequest BuildRequest(string path, string method = "GET", string body = "")
        {
            var nonce = Util.Nonce;
            var message = nonce + method + path + body;
            var sign = Util.GenerateNewHmac(_config.Secret, message);
            var req = RestUtil.CreateJsonRestRequest(path);
            req.Method = Util.ParseEnum<Method>(method);
            req.AddParameter("application/json", body, ParameterType.RequestBody);
            req.AddHeader("ACCESS-KEY", _config.Key);
            req.AddHeader("ACCESS-TIMESTAMP", nonce);
            req.AddHeader("ACCESS-SIGN", sign);
            return req;
        }
    }
}