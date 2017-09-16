using System.Timers;

namespace Rinjani
{
    public class TimerAdapter : ITimer
    {
        private readonly Timer _timer = new Timer();

        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public event ElapsedEventHandler Elapsed
        {
            add => _timer.Elapsed += value;
            remove => _timer.Elapsed -= value;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}