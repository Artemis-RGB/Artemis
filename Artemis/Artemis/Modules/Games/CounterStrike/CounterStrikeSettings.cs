using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeSettings : GameSettings
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

        public sealed override void Load()
        {
            Enabled = CounterStrike.Default.Enabled;
            GameDirectory = CounterStrike.Default.GameDirectory;

            AmmoEnabled = CounterStrike.Default.AmmoEnabled;
            AmmoMainColor = CounterStrike.Default.AmmoMainColor;
            AmmoSecondaryColor = CounterStrike.Default.AmmoSecondaryColor;

            TeamColorEnabled = CounterStrike.Default.TeamColorEnabled;
            FlashEnabled = CounterStrike.Default.FlashEnabled;
            SmokeEnabled = CounterStrike.Default.SmokeEnabled;
            LowHpEnabled = CounterStrike.Default.LowHpEnabled;
        }

        public sealed override void Save()
        {
            CounterStrike.Default.Enabled = Enabled;
            CounterStrike.Default.GameDirectory = GameDirectory;

            CounterStrike.Default.AmmoEnabled = AmmoEnabled;
            CounterStrike.Default.AmmoMainColor = AmmoMainColor;
            CounterStrike.Default.AmmoSecondaryColor = AmmoSecondaryColor;

            CounterStrike.Default.TeamColorEnabled = TeamColorEnabled;
            CounterStrike.Default.FlashEnabled = FlashEnabled;
            CounterStrike.Default.SmokeEnabled = SmokeEnabled;
            CounterStrike.Default.LowHpEnabled = LowHpEnabled;

            CounterStrike.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;

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