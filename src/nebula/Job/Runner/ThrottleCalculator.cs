using System;

namespace Nebula.Job.Runner
{
    /// <summary>
    /// Calculates wait times required for throttling any event.
    /// </summary>
    /// <remarks>
    /// Note: an instance of this class is NOT thread-safe, and is supposed
    /// to be called from a single scheduler thread. If a multi-threaded variation
    /// is required, you should either access this class in a Mutex, or consider
    /// SchedulerThread class in common-dotnet library.  
    /// </remarks>
    internal class ThrottleCalculator
    {
        private double _ratePerSecond;
        private int _maxBurstSize;
        private readonly int _initialQuota;

        private long _lastTick;
        private int _lastQuota;
        private long _ticksPerItem;

        public ThrottleCalculator(double ratePerSecond, int maxBurstSize = 1, int initialQuota = 0)
        {
            _ratePerSecond = ratePerSecond;
            _maxBurstSize = maxBurstSize;
            _initialQuota = initialQuota;

            ValidateAndFixSettings();
            CalculateRate();

            _lastTick = 0L;
            _lastQuota = 0;
        }

        public int WaitTimeForNextMillis
        {
            get
            {
                if (_lastTick <= 0L)
                    throw new InvalidOperationException("Throttle calculator is not started yet.");
                
                return RefreshLastQuota() > 0 ? 0 : CalculateWaitTimeMillisForNext();
            }
        }

        public int AvailableItems
        {
            get
            {
                if (_lastTick <= 0L)
                    throw new InvalidOperationException("Throttle calculator is not started yet.");
                
                return RefreshLastQuota();
            }
        }

        public void Start()
        {
            if (_lastTick > 0L)
                throw new InvalidOperationException("Throttle calculator is already started.");
                
            _lastQuota = _initialQuota;
            _lastTick = DateTime.UtcNow.Ticks;
        }

        public void UpdateRate(double newRatePerSecond)
        {
            if (_lastTick <= 0L)
                throw new InvalidOperationException("Throttle calculator is not started yet.");
                
            RefreshLastQuota();
            
            _ratePerSecond = newRatePerSecond;
            CalculateRate();
        }

        public int DecreaseQuota(int count = 1)
        {
            if (_lastTick <= 0L)
                throw new InvalidOperationException("Throttle calculator is not started yet.");
                
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");

            return ChangeQuota(-count);
        }
        
        public int IncreaseQuota(int count = 1)
        {
            if (_lastTick <= 0L)
                throw new InvalidOperationException("Throttle calculator is not started yet.");
                
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");

            return ChangeQuota(count);
        }

        public int ChangeQuota(int delta)
        {
            if (_lastTick <= 0L)
                throw new InvalidOperationException("Throttle calculator is not started yet.");
                
            _lastQuota += delta;
            return RefreshLastQuota();
        }

        private void ValidateAndFixSettings()
        {
            if (_ratePerSecond <= 0d)
                throw new ArgumentException($"{nameof(_ratePerSecond)} cannot be negative or zero.");

            _maxBurstSize = Math.Max(1, _maxBurstSize);
        }

        private int RefreshLastQuota()
        {
            var now = DateTime.UtcNow.Ticks;
            var ticksPassed = now - _lastTick;

            if (ticksPassed >= _ticksPerItem)
            {
                var itemCount = (int) (ticksPassed / _ticksPerItem);
                _lastQuota += itemCount;
                _lastTick += itemCount * _ticksPerItem;
            }

            _lastQuota = Math.Min(_maxBurstSize, _lastQuota);
            return _lastQuota;
        }

        private int CalculateWaitTimeMillisForNext()
        {
            if (_lastQuota > 0)
                return 0;
            
            var now = DateTime.UtcNow.Ticks;
            var ticksPassed = now - _lastTick;

            var waitTimeTicks = Math.Max(0L, _ticksPerItem - ticksPassed);
            return (int) (waitTimeTicks / TimeSpan.TicksPerMillisecond);
        }

        private void CalculateRate()
        {
            _ticksPerItem = (long) Math.Round(TimeSpan.TicksPerSecond/_ratePerSecond);
        }
    }
}