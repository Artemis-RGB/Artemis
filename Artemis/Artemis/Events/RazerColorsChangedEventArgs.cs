using System;
using System.Windows.Media;

namespace Artemis.Events
{
    public class RazerColorsChangedEventArgs : EventArgs
    {
        public RazerColorsChangedEventArgs(Color[,] colors)
        {
            Colors = colors;
        }

        public Color[,] Colors { get; }
    }
}