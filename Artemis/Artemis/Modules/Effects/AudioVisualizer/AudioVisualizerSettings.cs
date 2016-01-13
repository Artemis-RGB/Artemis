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
        public bool FromBottom { get; set; }
        public int FadeSpeed { get; set; }
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        public override sealed void Load()
        {
            Sensitivity = AudioVisualization.Default.Sensitivity;
            Bars = AudioVisualization.Default.Bars;
            FromBottom = AudioVisualization.Default.FromBottom;
            FadeSpeed = AudioVisualization.Default.FadeSpeed;
            MainColor = AudioVisualization.Default.MainColor;
            SecondaryColor = AudioVisualization.Default.SecondaryColor;
        }

        public override sealed void Save()
        {
            AudioVisualization.Default.Sensitivity = Sensitivity;
            AudioVisualization.Default.Bars = Bars;
            AudioVisualization.Default.FromBottom = FromBottom;
            AudioVisualization.Default.FadeSpeed = FadeSpeed;
            AudioVisualization.Default.MainColor = MainColor;
            AudioVisualization.Default.SecondaryColor = SecondaryColor;

            AudioVisualization.Default.Save();
        }

        public override sealed void ToDefault()
        {
            Sensitivity = 4;
            Bars = 21;
            FromBottom = true;
            FadeSpeed = 3;
            MainColor = Color.FromArgb(255, 0, 0, 255);
            SecondaryColor = Color.FromArgb(255, 30, 144, 255);
        }
    }
}