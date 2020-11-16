using System;
using System.Runtime.CompilerServices;

namespace Artemis.Core
{
    /// <summary>
    ///     A static class providing <see cref="double" /> extensions
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        ///     Rounds the provided number away to zero and casts the result to an <see cref="int" />
        /// </summary>
        /// <param name="number">The number to round</param>
        /// <returns>The rounded number as an integer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(this double number)
        {
            return (int) Math.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}