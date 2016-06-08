using Artemis.Models;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchSettings : GameSettings
    {
        public OverwatchSettings()
        {
            Load();
        }

        public string GameDirectory { get; set; }

        public sealed override void Load()
        {
            Enabled = Overwatch.Default.Enabled;
            LastProfile = Overwatch.Default.LastProfile;
            GameDirectory = Overwatch.Default.GameDirectory;
        }

        public sealed override void Save()
        {
            Overwatch.Default.Enabled = Enabled;
            Overwatch.Default.LastProfile = LastProfile;
            Overwatch.Default.GameDirectory = GameDirectory;

            Overwatch.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;
        }
    }
}