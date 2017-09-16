using System;
using System.Collections.Generic;

namespace Rinjani
{
    public interface IPositionService : IDisposable
    {
        decimal NetExposure { get; }
        IDictionary<Broker, BrokerPosition> PositionMap { get; }
    }
}