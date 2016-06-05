using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Modules.Games.RocketLeague;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        protected GameModel(MainManager mainManager, GameSettings settings, IGameDataModel gameDataModel) : base(mainManager)
        {
            Settings = settings;
            GameDataModel = gameDataModel;
        }

        public GameSettings Settings { get; set; }
        public bool Enabled { get; set; }
        public string ProcessName { get; set; }
        public IGameDataModel GameDataModel { get; set; }
        public ProfileModel Profile { get; set; }
    }
}