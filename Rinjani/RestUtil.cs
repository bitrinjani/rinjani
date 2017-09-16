using System;
using System.Net;
using NLog;
using RestSharp;

namespace Rinjani
{
    public static class RestUtil
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static RestRequest CreateJsonRestRequest(string path, bool addHeader = true)
        {
            var request = new RestRequest(path)
            {
                RequestFormat = DataFormat.Json
            };
            if (addHeader)
            {
                request.AddHeader("Content-Type", "application/json");
            }
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            return request;
        }

        public static T ExecuteRequest<T>(IRestClient restClient, RestRequest req) where T : new()
        {
            LogRestRequest(req);
            var response = restClient.Execute<T>(req);
            CheckError(response);
            return response.Data;
        }

        public static void ExecuteRequest(IRestClient restClient, RestRequest req)
        {
            LogRestRequest(req);
            var response = restClient.Execute(req);
            CheckError(response);
        }

        private static void LogRestRequest(IRestRequest req)
        {
            Log.Debug($"{req.Method} {req.Resource}");
            foreach (var p in req.Parameters)
            {
                Log.Debug($"{p.Name}, {p.ContentType}, {p.Value}");
            }
        }

        public static void CheckError(IRestResponse response)
        {
            Log.Debug($"{response.StatusCode}, {response.Content}, {response.ErrorMessage}");
            if (response.ErrorException != null)
            {
                Log.Error(response.ErrorException);
                throw response.ErrorException;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error(response.StatusCode);
                Log.Error(response.Content);
                throw new InvalidOperationException(response.Content);
            }
        }
    }
}