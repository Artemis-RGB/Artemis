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
            Enabled = Settings.VolumeDisplay.Default.Enabled;
            MainColor = Settings.VolumeDisplay.Default.MainColor;
            SecondaryColor = Settings.VolumeDisplay.Default.SecondaryColor;
        }

        public sealed override void Save()
        {
            Settings.VolumeDisplay.Default.Enabled = Enabled;
            Settings.VolumeDisplay.Default.MainColor = MainColor;
            Settings.VolumeDisplay.Default.SecondaryColor = SecondaryColor;

            Settings.VolumeDisplay.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            MainColor = Color.FromArgb(255, 38, 246, 0);
            SecondaryColor = Color.FromArgb(255, 255, 41, 0);
        }
    }
}