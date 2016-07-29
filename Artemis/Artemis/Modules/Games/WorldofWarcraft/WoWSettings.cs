using Artemis.Models;

namespace Artemis.Modules.Games.WorldofWarcraft
{
    public class WoWSettings : GameSettings
    {
        public WoWSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = WoW.Default.Enabled;
            LastProfile = WoW.Default.LastProfile;
        }

        public sealed override void Save()
        {
            WoW.Default.Enabled = Enabled;
            WoW.Default.LastProfile = LastProfile;

            WoW.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}