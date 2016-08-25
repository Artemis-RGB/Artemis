using System.Windows.Media;
using Artemis.Settings;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplaySettings : OverlaySettings
    {
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }
    }
}