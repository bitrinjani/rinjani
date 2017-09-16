using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rinjani.Tests
{
    [TestClass]
    [DeploymentItem(ConfigPath)]
    public class UtilTest
    {
        private const string ConfigPath = "config_test.json";

        [TestMethod]
        public void GenerateNewHmacTest()
        {
            var key = "test";
            var message = "aaa";
            var sig = Util.GenerateNewHmac(key, message);
            Assert.AreEqual(sig, "d082cc8258ec12d2609d94f2bf4e6866231e4e29fcebf6b81c62d4641480743b");
        }

        [TestMethod]
        public void MergeTest()
        {
            Quote NewQuote(decimal i)
            {
                return new Quote {Price = i, Volume = 1, Broker = Broker.Bitflyer, Side = QuoteSide.Ask};
            }

            var quotes = new List<Quote>
            {
                NewQuote(1),
                NewQuote(2),
                NewQuote(11),
                NewQuote(512),
                NewQuote(514),
                NewQuote(510)
            };

            var step = 5;
            var r = quotes.Merge(step).OrderBy(q => q.Price).ToList();
            Assert.AreEqual(5, r[0].Price);
            Assert.AreEqual(2, r[0].Volume);
            Assert.AreEqual(15, r[1].Price);
            Assert.AreEqual(1, r[1].Volume);
            Assert.AreEqual(510, r[2].Price);
            Assert.AreEqual(1, r[2].Volume);
            Assert.AreEqual(515, r[3].Price);
            Assert.AreEqual(2, r[3].Volume);
        }

        [TestMethod]
        public void MergeTest2()
        {
            var quotes = new List<Quote>
            {
                new Quote(Broker.Quoine, QuoteSide.Ask, 14, (decimal) 0.1),
                new Quote(Broker.Quoine, QuoteSide.Ask, 11, (decimal) 0.2),
                new Quote(Broker.Quoine, QuoteSide.Bid, 12, (decimal) 0.1),
                new Quote(Broker.Bitflyer, QuoteSide.Ask, 11, (decimal) 0.01),
                new Quote(Broker.Bitflyer, QuoteSide.Bid, 13, (decimal) 0.01)
            };

            var step = 5;
            var r = quotes.Merge(step).OrderBy(q => q.Price).ToList();
            Assert.AreEqual(4, r.Count);
            var ql = r.Where(q => q.Broker == Broker.Quoine && q.Side == QuoteSide.Ask && q.Price == 15).ToList();
            Assert.AreEqual(1, ql.Count);
            Assert.AreEqual(ql[0].Volume, (decimal) 0.3);
        }

        [TestMethod]
        public void MergePerfTest()
        {
            Quote NewQuote(decimal i)
            {
                return new Quote {Price = i, Volume = 1, Broker = Broker.Bitflyer, Side = QuoteSide.Ask};
            }

            var quotes = new List<Quote>();
            foreach (var i in Enumerable.Range(0, 10000))
            {
                quotes.AddRange(new[] {NewQuote(1), NewQuote(2), NewQuote(11)});
            }

            MergePerf(quotes);
        }

        private void MergePerf(IList<Quote> quotes)
        {
            var step = 5;
            var l = new List<long>(10);
            foreach (var i in Enumerable.Range(0, 10))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var r = quotes.Merge(step).ToList();
                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;
                Debug.WriteLine(elapsed);
                l.Add(elapsed);
            }

            var avg = l.Average();
            Debug.WriteLine($"AVG: {avg}");
        }
    }
}