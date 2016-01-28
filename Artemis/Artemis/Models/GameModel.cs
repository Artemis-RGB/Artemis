namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        public bool Enabled;
        public string ProcessName;

        protected GameModel(MainModel mainModel) : base(mainModel)
        {
        }
    }
}