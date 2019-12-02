using System;

namespace Artemis.Plugins.Modules.General
{
    public static class ColorHelpers
    {
        private static readonly Random Rand = new Random();

        public static int GetRandomHue()
        {
            return Rand.Next(0, 360);
        }
    }
}