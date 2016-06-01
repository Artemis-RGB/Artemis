using Artemis.Models;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3Settings : GameSettings
    {
        public Witcher3Settings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = Witcher3.Default.Enabled;
            LastProfile = Witcher3.Default.LastProfile;
        }

        public sealed override void Save()
        {
            Witcher3.Default.Enabled = Enabled;
            Witcher3.Default.LastProfile = LastProfile;

            Witcher3.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}