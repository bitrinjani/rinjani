using System.Collections.Generic;

namespace Rinjani
{
    public interface IBrokerAdapter
    {
        Broker Broker { get; }
        void Send(Order order);
        void Refresh(Order order);
        void Cancel(Order order);
        decimal GetBtcPosition();
        IList<Quote> FetchQuotes();
    }
}