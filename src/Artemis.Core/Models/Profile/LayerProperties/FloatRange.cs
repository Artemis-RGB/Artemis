using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a range between two single-precision floating point numbers
    /// </summary>
    public class FloatRange
    {
        private readonly Random _rand;

        /// <summary>
        ///     Creates a new instance of the <see cref="FloatRange" /> class
        /// </summary>
        public FloatRange()
        {
            _rand = new Random();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="FloatRange" /> class
        /// </summary>
        /// <param name="start">The start value of the range</param>
        /// <param name="end">The end value of the range</param>
        public FloatRange(float start, float end)
        {
            Start = start;
            End = end;

            _rand = new Random();
        }

        /// <summary>
        ///     Gets or sets the start value of the range
        /// </summary>
        public float Start { get; set; }

        /// <summary>
        ///     Gets or sets the end value of the range
        /// </summary>
        public float End { get; set; }

        /// <summary>
        ///     Determines whether the given value is in this range
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="inclusive">
        ///     Whether the value may be equal to <see cref="Start" /> or <see cref="End" />
        ///     <para>Defaults to <see langword="true" /></para>
        /// </param>
        /// <returns></returns>
        public bool IsInRange(float value, bool inclusive = true)
        {
            if (inclusive)
                return value >= Start && value <= End;
            return value > Start && value < End;
        }

        /// <summary>
        ///     Returns a pseudo-random value between <see cref="Start" /> and <see cref="End" />
        /// </summary>
        /// <param name="inclusive">Whether the value may be equal to <see cref="Start" /></param>
        /// <returns>The pseudo-random value</returns>
        public float GetRandomValue(bool inclusive = true)
        {
            if (inclusive)
                return _rand.Next((int) (Start * 100), (int) (End * 100)) / 100f;
            return _rand.Next((int) (Start * 100) + 1, (int) (End * 100)) / 100f;
        }
    }
}