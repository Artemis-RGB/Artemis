using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        protected GameModel(MainManager mainManager, GameSettings settings) : base(mainManager)
        {
            Settings = settings;
        }

        public GameSettings Settings { get; set; }
        public bool Enabled { get; set; }
        public string ProcessName { get; set; }
        public IGameDataModel GameDataModel { get; set; }
        public ProfileModel Profile { get; set; }
    }
}