using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveSettings : EffectSettings
    {
        public TypeWaveSettings()
        {
            Load();
        }

        public bool IsRandomColors { get; set; }
        public Color WaveColor { get; set; }
        public bool IsShiftColors { get; set; }
        public int ShiftColorSpeed { get; set; }
        public int TimeToLive { get; set; }
        public int SpreadSpeed { get; set; }

        public sealed override void Load()
        {
            IsRandomColors = TypeWave.Default.IsRandomColors;
            WaveColor = TypeWave.Default.WaveColor;
            IsShiftColors = TypeWave.Default.IsShiftColors;
            ShiftColorSpeed = TypeWave.Default.ShiftColorSpeed;
            TimeToLive = TypeWave.Default.TimeToLive;
            SpreadSpeed = TypeWave.Default.SpreadSpeed;
        }

        public sealed override void Save()
        {
            TypeWave.Default.IsRandomColors = IsRandomColors;
            TypeWave.Default.WaveColor = WaveColor;
            TypeWave.Default.IsShiftColors = IsShiftColors;
            TypeWave.Default.ShiftColorSpeed = ShiftColorSpeed;
            TypeWave.Default.TimeToLive = TimeToLive;
            TypeWave.Default.SpreadSpeed = SpreadSpeed;

            TypeWave.Default.Save();
        }

        public sealed override void ToDefault()
        {
            IsRandomColors = true;
            WaveColor = Color.FromArgb(255, 255, 0, 0);
            IsShiftColors = true;
            ShiftColorSpeed = 20;
            TimeToLive = 500;
            SpreadSpeed = 4;
        }
    }
}