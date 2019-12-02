using System;

namespace Artemis.Core.Extensions
{
    public static class DoubleExtensions
    {
        public static int RoundToInt(this double number)
        {
            return (int) Math.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}