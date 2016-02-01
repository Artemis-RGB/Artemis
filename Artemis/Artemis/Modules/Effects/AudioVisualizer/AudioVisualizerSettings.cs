using System.Windows.Media;
using Artemis.Models;

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
        public Color TopColor { get; set; }
        public Color MiddleColor { get; set; }
        public Color BottomColor { get; set; }

        public override sealed void Load()
        {
            Sensitivity = AudioVisualization.Default.Sensitivity;
            Bars = AudioVisualization.Default.Bars;
            FromBottom = AudioVisualization.Default.FromBottom;
            FadeSpeed = AudioVisualization.Default.FadeSpeed;
            TopColor = AudioVisualization.Default.TopColor;
            MiddleColor = AudioVisualization.Default.MiddleColor;
            BottomColor = AudioVisualization.Default.BottomColor;
        }

        public override sealed void Save()
        {
            AudioVisualization.Default.Sensitivity = Sensitivity;
            AudioVisualization.Default.Bars = Bars;
            AudioVisualization.Default.FromBottom = FromBottom;
            AudioVisualization.Default.FadeSpeed = FadeSpeed;
            AudioVisualization.Default.TopColor = TopColor;
            AudioVisualization.Default.MiddleColor = MiddleColor;
            AudioVisualization.Default.BottomColor = BottomColor;

            AudioVisualization.Default.Save();
        }

        public override sealed void ToDefault()
        {
            Sensitivity = 4;
            Bars = 21;
            FromBottom = true;
            FadeSpeed = 3;
            TopColor = Color.FromArgb(255, 249, 0, 0);
            MiddleColor = Color.FromArgb(255, 255, 118, 30);
            BottomColor = Color.FromArgb(255, 0, 223, 0);
        }
    }
}