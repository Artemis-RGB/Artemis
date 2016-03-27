using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplaySettings : EffectSettings
    {
        public VolumeDisplaySettings()
        {
            Load();
        }

        public bool Enabled { get; set; }
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        public sealed override void Load()
        {
            Enabled = VolumeDisplay.Default.Enabled;
            MainColor = VolumeDisplay.Default.MainColor;
            SecondaryColor = VolumeDisplay.Default.SecondaryColor;
        }

        public sealed override void Save()
        {
            VolumeDisplay.Default.Enabled = Enabled;
            VolumeDisplay.Default.MainColor = MainColor;
            VolumeDisplay.Default.SecondaryColor = SecondaryColor;

            VolumeDisplay.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            MainColor = Color.FromArgb(255, 38, 246, 0);
            SecondaryColor = Color.FromArgb(255, 255, 41, 0);
        }
    }
}