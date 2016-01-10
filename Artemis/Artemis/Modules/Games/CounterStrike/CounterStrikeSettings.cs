using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeSettings : EffectSettings
    {
        public CounterStrikeSettings()
        {
            Load();
        }

        public string GameDirectory { get; set; }

        public bool AmmoEnabled { get; set; }
        public Color AmmoMainColor { get; set; }
        public Color AmmoSecondaryColor { get; set; }

        public bool TeamColorEnabled { get; set; }
        public bool FlashEnabled { get; set; }
        public bool SmokeEnabled { get; set; }
        public bool LowHpEnabled { get; set; }

        public override sealed void Load()
        {
            GameDirectory = Settings.CounterStrike.Default.GameDirectory;

            AmmoEnabled = Settings.CounterStrike.Default.AmmoEnabled;
            AmmoMainColor = Settings.CounterStrike.Default.AmmoMainColor;
            AmmoSecondaryColor = Settings.CounterStrike.Default.AmmoSecondaryColor;

            TeamColorEnabled = Settings.CounterStrike.Default.TeamColorEnabled;
            FlashEnabled = Settings.CounterStrike.Default.FlashEnabled;
            SmokeEnabled = Settings.CounterStrike.Default.SmokeEnabled;
            LowHpEnabled = Settings.CounterStrike.Default.LowHpEnabled;
        }

        public override sealed void Save()
        {
            Settings.CounterStrike.Default.GameDirectory = GameDirectory;

            Settings.CounterStrike.Default.AmmoEnabled = AmmoEnabled;
            Settings.CounterStrike.Default.AmmoMainColor = AmmoMainColor;
            Settings.CounterStrike.Default.AmmoSecondaryColor = AmmoSecondaryColor;

            Settings.CounterStrike.Default.TeamColorEnabled = TeamColorEnabled;
            Settings.CounterStrike.Default.FlashEnabled = FlashEnabled;
            Settings.CounterStrike.Default.SmokeEnabled = SmokeEnabled;
            Settings.CounterStrike.Default.LowHpEnabled = LowHpEnabled;

            Settings.CounterStrike.Default.Save();
        }

        public override sealed void ToDefault()
        {
            GameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive";

            AmmoEnabled = true;
            AmmoMainColor = Color.FromArgb(255, 38, 246, 0);
            AmmoSecondaryColor = Color.FromArgb(255, 255, 41, 0);

            TeamColorEnabled = true;
            FlashEnabled = true;
            SmokeEnabled = true;
            LowHpEnabled = true;
        }
    }
}