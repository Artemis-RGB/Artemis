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

        public sealed override void Load()
        {
            Enabled = CounterStrike.Default.Enabled;
            LastProfile = CounterStrike.Default.LastProfile;
            GameDirectory = CounterStrike.Default.GameDirectory;
        }

        public sealed override void Save()
        {
            CounterStrike.Default.Enabled = Enabled;
            CounterStrike.Default.GameDirectory = GameDirectory;

            CounterStrike.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;
        }
    }
}