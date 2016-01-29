using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueSettings : EffectSettings
    {
        public RocketLeagueSettings()
        {
            Load();
        }

        public bool Enabled { get; set; }
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        public override sealed void Load()
        {
            Enabled = RocketLeague.Default.Enabled;
            MainColor = RocketLeague.Default.MainColor;
            SecondaryColor = RocketLeague.Default.SecondaryColor;
        }

        public override sealed void Save()
        {
            RocketLeague.Default.Enabled = Enabled;
            RocketLeague.Default.MainColor = MainColor;
            RocketLeague.Default.SecondaryColor = SecondaryColor;

            RocketLeague.Default.Save();
        }

        public override sealed void ToDefault()
        {
            Enabled = true;
            MainColor = Color.FromArgb(255, 255, 80, 0);
            SecondaryColor = Color.FromArgb(255, 255, 0, 0);
        }
    }
}