using Artemis.Models;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueSettings : GameSettings
    {
        public RocketLeagueSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = RocketLeague.Default.Enabled;
            LastProfile = RocketLeague.Default.LastProfile;
        }

        public sealed override void Save()
        {
            RocketLeague.Default.Enabled = Enabled;
            RocketLeague.Default.LastProfile = LastProfile;

            RocketLeague.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}