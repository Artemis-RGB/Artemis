using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool _open;
        private long _start;

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
            _start = Stopwatch.GetTimestamp();
            _open = true;
        }

        /// <summary>
        ///     Stops measuring time and stores the time passed in the <see cref="Measurements" /> list
        /// </summary>
        /// <param name="correction">An optional correction in ticks to subtract from the measurement</param>
        /// <returns>The time passed since the last <see cref="Start" /> call</returns>
        public long Stop(long correction = 0)
        {
            if (!_open)
                return 0;

            long difference = Stopwatch.GetTimestamp() - _start - correction;
            _open = false;
            Measurements[_index] = difference;

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

        /// <summary>
        ///     Gets the nth percentile of the last 1000 measurements
        /// </summary>
        public TimeSpan GetPercentile(double percentile)
        {
            if (!_filledArray && _index == 0)
                return TimeSpan.Zero;

            long[] collection = _filledArray
                ? Measurements.OrderBy(l => l).ToArray()
                : Measurements.Take(_index).OrderBy(l => l).ToArray();

            return new TimeSpan((long) Percentile(collection, percentile));
        }

        private static double Percentile(long[] elements, double percentile)
        {
            Array.Sort(elements);
            double realIndex = percentile * (elements.Length - 1);
            int index = (int) realIndex;
            double frac = realIndex - index;
            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            return elements[index];
        }
    }
}