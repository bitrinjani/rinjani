using System;
using System.Collections.Generic;

namespace Rinjani
{
    public interface IQuoteAggregator : IDisposable
    {
        IList<Quote> Quotes { get; }
        event EventHandler QuoteUpdated;
    }
}