using System;
using System.Runtime.CompilerServices;

namespace Artemis.Core
{
    public static class DoubleExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(this double number)
        {
            return (int) Math.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}