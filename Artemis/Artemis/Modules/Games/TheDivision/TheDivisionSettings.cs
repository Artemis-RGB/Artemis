using Artemis.Models;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionSettings : GameSettings
    {
        public TheDivisionSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            Enabled = TheDivision.Default.Enabled;
        }

        public sealed override void Save()
        {
            TheDivision.Default.Enabled = Enabled;

            TheDivision.Default.Save();
        }

        public sealed override void ToDefault()
        {
            Enabled = true;
        }
    }
}