using System;
using System.Runtime.CompilerServices;

namespace Artemis.Core
{
    /// <summary>
    ///     A static class providing <see cref="float" /> extensions
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        ///     Rounds the provided number away to zero and casts the result to an <see cref="int" />
        /// </summary>
        /// <param name="number">The number to round</param>
        /// <returns>The rounded number as an integer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(this float number)
        {
            return (int) MathF.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}