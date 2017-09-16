using System.Collections.Generic;
using System.Linq;

namespace Rinjani.Quoine
{
    public class Depth
    {
        public List<List<decimal>> BuyPriceLevels { get; set; }
        public List<List<decimal>> SellPriceLevels { get; set; }

        public IList<Quote> ToQuotes()
        {
            var quotes = new List<Quote>();
            if (BuyPriceLevels != null)
            {
                quotes.AddRange(BuyPriceLevels.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Quoine,
                    Side = QuoteSide.Bid,
                    Price = x[0],
                    Volume = x[1]
                }));
            }
            if (SellPriceLevels != null)
            {
                quotes.AddRange(SellPriceLevels.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Quoine,
                    Side = QuoteSide.Ask,
                    Price = x[0],
                    Volume = x[1]
                }));
            }
            return quotes;
        }
    }
}