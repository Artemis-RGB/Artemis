using System;
using System.Linq;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a set of profiling measurements
    /// </summary>
    public class ProfilingMeasurement
    {
        private bool _filledArray;
        private int _index;
        private long _last;
        private DateTime? _start;

        internal ProfilingMeasurement(string identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        ///     Gets the unique identifier of this measurement
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the last 1000 measurements
        /// </summary>
        public long[] Measurements { get; } = new long[1000];

        /// <summary>
        ///     Starts measuring time until <see cref="Stop" /> is called
        /// </summary>
        public void Start()
        {
            _start = DateTime.UtcNow;
        }

        /// <summary>
        ///     Stops measuring time and stores the time passed in the <see cref="Measurements" /> list
        /// </summary>
        /// <returns>The time passed since the last <see cref="Start" /> call</returns>
        public long Stop()
        {
            if (_start == null)
                return 0;
            
            long difference = (DateTime.UtcNow - _start.Value).Ticks;
            Measurements[_index] = difference;
            _start = null;
            
            _index++;
            if (_index >= 1000)
            {
                _filledArray = true;
                _index = 0;
            }

            _last = difference;
            return difference;
        }

        /// <summary>
        ///     Gets the last measured time
        /// </summary>
        public TimeSpan GetLast()
        {
            return new(_last);
        }

        /// <summary>
        ///     Gets the average time of the last 1000 measurements
        /// </summary>
        public TimeSpan GetAverage()
        {
            if (!_filledArray && _index == 0)
                return TimeSpan.Zero;

            return _filledArray
                ? new TimeSpan((long) Measurements.Average(m => m))
                : new TimeSpan((long) Measurements.Take(_index).Average(m => m));
        }

        /// <summary>
        ///     Gets the min time of the last 1000 measurements
        /// </summary>
        public TimeSpan GetMin()
        {
            if (!_filledArray && _index == 0)
                return TimeSpan.Zero;

            return _filledArray
                ? new TimeSpan(Measurements.Min())
                : new TimeSpan(Measurements.Take(_index).Min());
        }

        /// <summary>
        ///     Gets the max time of the last 1000 measurements
        /// </summary>
        public TimeSpan GetMax()
        {
            if (!_filledArray && _index == 0)
                return TimeSpan.Zero;

            return _filledArray
                ? new TimeSpan(Measurements.Max())
                : new TimeSpan(Measurements.Take(_index).Max());
        }
    }
}