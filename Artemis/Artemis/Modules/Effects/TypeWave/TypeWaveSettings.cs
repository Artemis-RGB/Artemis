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

        public override sealed void Load()
        {
            IsRandomColors = Settings.TypeWave.Default.IsRandomColors;
            WaveColor = Settings.TypeWave.Default.WaveColor;
            IsShiftColors = Settings.TypeWave.Default.IsShiftColors;
            ShiftColorSpeed = Settings.TypeWave.Default.ShiftColorSpeed;
            TimeToLive = Settings.TypeWave.Default.TimeToLive;
            SpreadSpeed = Settings.TypeWave.Default.SpreadSpeed;
        }

        public override sealed void Save()
        {
            Settings.TypeWave.Default.IsRandomColors = IsRandomColors;
            Settings.TypeWave.Default.WaveColor = WaveColor;
            Settings.TypeWave.Default.IsShiftColors = IsShiftColors;
            Settings.TypeWave.Default.ShiftColorSpeed = ShiftColorSpeed;
            Settings.TypeWave.Default.TimeToLive = TimeToLive;
            Settings.TypeWave.Default.SpreadSpeed = SpreadSpeed;

            Settings.TypeWave.Default.Save();
        }

        public override sealed void ToDefault()
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