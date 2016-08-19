using Artemis.Models;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2Settings : GameSettings
    {
        public EurotruckSimulator2Settings()
        {
            Load();
        }

        public string GameDirectory { get; set; }

        public sealed override void Load()
        {
            Enabled = EurotruckSimulator2.Default.Enabled;
            LastProfile = EurotruckSimulator2.Default.LastProfile;
            GameDirectory = EurotruckSimulator2.Default.GameDirectory;
        }

        public sealed override void Save()
        {
            EurotruckSimulator2.Default.Enabled = Enabled;
            EurotruckSimulator2.Default.GameDirectory = GameDirectory;

            EurotruckSimulator2.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;
        }
    }
}