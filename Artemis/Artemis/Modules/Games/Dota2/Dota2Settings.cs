using Artemis.Models;

namespace Artemis.Modules.Games.Dota2
{
    internal class Dota2Settings : GameSettings
    {
        public Dota2Settings()
        {
            Load();
        }

        public string GameDirectory { get; set; }


        public sealed override void Load()
        {
            Enabled = Dota2.Default.Enabled;
            GameDirectory = Dota2.Default.GameDirectory;
        }

        public sealed override void Save()
        {
            Dota2.Default.Enabled = Enabled;
            Dota2.Default.LastProfile = LastProfile;
            Dota2.Default.GameDirectory = GameDirectory;

            Dota2.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;
        }
    }
}