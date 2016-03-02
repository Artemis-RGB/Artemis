using Artemis.Managers;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        public bool Enabled;
        public string ProcessName;

        protected GameModel(MainManager mainManager) : base(mainManager)
        {
        }
    }
}