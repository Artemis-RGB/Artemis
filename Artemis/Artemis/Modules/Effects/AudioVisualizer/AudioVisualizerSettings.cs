using System.Windows.Media;
using Artemis.Models;
using Artemis.Settings;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public class AudioVisualizerSettings : EffectSettings
    {
        public AudioVisualizerSettings()
        {
            Load();
        }

        public int Sensitivity { get; set; }
        public int Bars { get; set; }
        public int Spread { get; set; }
        public int FadeSpeed { get; set; }
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        public override sealed void Load()
        {
            Sensitivity = AudioVisualization.Default.Sensitivity;
            Bars = AudioVisualization.Default.Bars;
            Spread = AudioVisualization.Default.Spread;
            FadeSpeed = AudioVisualization.Default.FadeSpeed;
            MainColor = AudioVisualization.Default.MainColor;
            SecondaryColor = AudioVisualization.Default.SecondaryColor;
        }

        public override sealed void Save()
        {
            AudioVisualization.Default.Sensitivity = Sensitivity;
            AudioVisualization.Default.Bars = Bars;
            AudioVisualization.Default.Spread = Spread;
            AudioVisualization.Default.FadeSpeed = FadeSpeed;
            AudioVisualization.Default.MainColor = MainColor;
            AudioVisualization.Default.SecondaryColor = SecondaryColor;

            AudioVisualization.Default.Save();
        }

        public override sealed void ToDefault()
        {
            Sensitivity = 4;
            Bars = 21;
            Spread = 1;
            FadeSpeed = 3;
            MainColor = Color.FromArgb(255, 0, 0, 255);
            SecondaryColor = Color.FromArgb(255, 30, 144, 255);
        }
    }
}