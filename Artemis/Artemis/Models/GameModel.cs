using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        public bool Enabled { get; set; }
        public string ProcessName { get; set; }
        public IGameDataModel GameDataModel { get; set; }
        public ProfileModel Profile { get; set; }

        protected GameModel(MainManager mainManager) : base(mainManager)
        {
        }
    }
}