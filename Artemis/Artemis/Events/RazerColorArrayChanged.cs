using System.Windows.Media;

namespace Artemis.Events
{
    public class RazerColorArrayChanged
    {
        public RazerColorArrayChanged(Color[,] colors)
        {
            Colors = colors;
        }

        public Color[,] Colors { get; set; }
    }
}