using System.Collections.Generic;
using System.Linq;

namespace Rinjani.Coincheck
{
    public class Depth
    {
        public List<List<decimal>> Asks { get; set; }
        public List<List<decimal>> Bids { get; set; }

        public IList<Quote> ToQuotes()
        {
            var quotes = new List<Quote>();
            if (Asks != null)
            {
                quotes.AddRange(Asks?.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Coincheck,
                    Side = QuoteSide.Ask,
                    Price = x[0],
                    Volume = x[1]
                }));
            }
            if (Bids != null)
            {
                quotes.AddRange(Bids?.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Coincheck,
                    Side = QuoteSide.Bid,
                    Price = x[0],
                    Volume = x[1]
                }));
            }
            return quotes;
        }
    }
}