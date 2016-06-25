using System;
using System.Windows.Media;

namespace Artemis.Events
{
    public class RazerColorArrayChanged
    {
        public Color[,] Colors { get; set; }

        public RazerColorArrayChanged(Color[,] colors)
        {
            Colors = colors;
        }
    }
}