namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        public abstract bool Enabled();
        public string ProcessName;

        protected GameModel(MainModel mainModel) : base(mainModel)
        {
        }
    }
}