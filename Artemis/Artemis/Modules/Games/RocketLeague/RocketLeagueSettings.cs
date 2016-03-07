using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueSettings : GameSettings
    {
        public RocketLeagueSettings()
        {
            Load();
        }

        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }
        public bool ContextualColor { get; set; }

        public sealed override void Load()
        {
            Enabled = RocketLeague.Default.Enabled;
            MainColor = RocketLeague.Default.MainColor;
            SecondaryColor = RocketLeague.Default.SecondaryColor;
            ContextualColor = RocketLeague.Default.ContextualColor;
        }

        public sealed override void Save()
        {
            RocketLeague.Default.Enabled = Enabled;
            RocketLeague.Default.MainColor = MainColor;
            RocketLeague.Default.SecondaryColor = SecondaryColor;
            RocketLeague.Default.ContextualColor = ContextualColor;

            RocketLeague.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
            MainColor = Color.FromArgb(255, 255, 80, 0);
            SecondaryColor = Color.FromArgb(255, 255, 0, 0);
            ContextualColor = false;
        }
    }
}