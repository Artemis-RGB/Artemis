using Artemis.Models;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchSettings : GameSettings
    {
        public OverwatchSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = Overwatch.Default.Enabled;
        }

        public sealed override void Save()
        {
            Overwatch.Default.Enabled = Enabled;

            Overwatch.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}