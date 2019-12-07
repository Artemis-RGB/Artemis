using System;

namespace Artemis.Core.Extensions
{
    public static class FloatExtensions
    {
        public static int RoundToInt(this float number)
        {
            return (int)Math.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}