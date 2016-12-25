using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Settings;

namespace Artemis.Models
{
    public abstract class GameModel : EffectModel
    {
        protected GameModel(DeviceManager deviceManager, LuaManager luaManager, GameSettings settings,
            IDataModel dataModel) : base(deviceManager, luaManager, settings, dataModel)
        {
            // Override settings to the GameSettings type
            Settings = settings;
        }

        public new GameSettings Settings { get; set; }
        public bool Enabled { get; set; }
        public string ProcessName { get; set; }
    }
}