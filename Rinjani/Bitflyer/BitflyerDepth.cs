using System.Collections.Generic;
using System.Linq;

namespace Rinjani
{
    public class BitflyerDepth
    {
        public List<PriceSizePair> Bids { get; set; }
        public List<PriceSizePair> Asks { get; set; }

        public IList<Quote> ToQuotes()
        {
            var quotes = new List<Quote>();
            if (Asks != null)
            {
                quotes.AddRange(Asks.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Bitflyer,
                    Side = QuoteSide.Ask,
                    Price = x.Price,
                    Volume = x.Size
                }));
            }
            if (Bids != null)
            {
                quotes.AddRange(Bids.Take(100).Select(x => new Quote
                {
                    Broker = Broker.Bitflyer,
                    Side = QuoteSide.Bid,
                    Price = x.Price,
                    Volume = x.Size
                }));
            }
            return quotes;
        }

        public class PriceSizePair
        {
            public decimal Price { get; set; }
            public decimal Size { get; set; }
        }
    }
}