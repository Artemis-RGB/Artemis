using Artemis.Managers;
using Artemis.Models.Interfaces;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        protected GameModel(MainManager mainManager, GameSettings settings, IDataModel dataModel): base(mainManager, settings, dataModel)
        {
            // Override settings to the GameSettings type
            Settings = settings;
        }

        public new GameSettings Settings { get; set; }
        public bool Enabled { get; set; }
        public string ProcessName { get; set; }
    }
}