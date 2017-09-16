using System;
using System.Timers;

namespace Rinjani
{
    public interface ITimer : IDisposable
    {
        double Interval { get; set; }
        event ElapsedEventHandler Elapsed;
        void Start();
        void Stop();
    }
}