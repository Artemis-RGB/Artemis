using Artemis.Models;

namespace Artemis.Modules.Games.UnrealTournament
{
    public class UnrealTournamentSettings : GameSettings
    {
        public UnrealTournamentSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = UnrealTournament.Default.Enabled;
            LastProfile = UnrealTournament.Default.LastProfile;
        }

        public sealed override void Save()
        {
            UnrealTournament.Default.Enabled = Enabled;
            UnrealTournament.Default.LastProfile = LastProfile;

            UnrealTournament.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}