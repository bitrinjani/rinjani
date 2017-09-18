using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using Jose;

namespace Rinjani
{
    public static class Util
    {
        public static IList<IBrokerAdapter> GetEnabledBrokerAdapters(IList<IBrokerAdapter> brokerAdapters,
            IConfigStore configStore)
        {
            var enabledBrokers = configStore.Config.Brokers.Where(b => b.Enabled).Select(b => b.Broker).ToList();
            return brokerAdapters.Where(b => enabledBrokers.Contains(b.Broker)).ToList();
        }

        public static string Nonce => Convert.ToString(DateTime.UtcNow.ToUnixMs());

        public static DateTime IsoDateTimeToLocal(string isoTime)
        {
            return DateTimeOffset.Parse(isoTime, null, DateTimeStyles.RoundtripKind).LocalDateTime;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string Hr(int width)
        {
            return string.Concat(Enumerable.Repeat("-", width));
        }

        public static void StartTimer(ITimer timer, double interval, ElapsedEventHandler handler)
        {
            timer.Interval = interval;
            timer.Elapsed += handler;
            timer.Start();
        }

        public static long ToUnixMs(this DateTime dt)
        {
            return (long) dt.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
        }

        public static string GenerateNewHmac(string secret, string message)
        {
            var hmc = new HMACSHA256(Encoding.ASCII.GetBytes(secret));
            var hmres = hmc.ComputeHash(Encoding.ASCII.GetBytes(message));
            return BitConverter.ToString(hmres).Replace("-", "").ToLower();
        }

        public static IEnumerable<Quote> Merge(this IEnumerable<Quote> quotes, int step)
        {
            return quotes.ToLookup(q =>
                {
                    var price = q.Side == QuoteSide.Ask
                        ? Math.Ceiling(q.Price / step) * step
                        : Math.Floor(q.Price / step) * step;
                    return new QuoteKey(q.Broker, q.Side, price);
                })
                .Select(l => new Quote(l.Key.Broker, l.Key.Side, l.Key.Price, l.Sum(q => q.Volume)));
        }

        public static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static string JwtHs256Encode(object payload, string secret)
        {
            var secbyte = Encoding.UTF8.GetBytes(secret);
            return JWT.Encode(payload, secbyte, JwsAlgorithm.HS256);
        }

        public static IEnumerable<T> Enums<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static decimal RoundDown(decimal d, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(d * power) / power;
        }
    }
}