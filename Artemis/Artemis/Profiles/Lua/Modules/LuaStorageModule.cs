using Artemis.DAL;
using Artemis.Managers;
using Artemis.Settings;
using MoonSharp.Interpreter;
using static MoonSharp.Interpreter.Serialization.Json.JsonTableConverter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaStorageModule : LuaModule
    {
        private readonly Table _globalValues;
        private readonly Table _profileValues;
        private readonly LuaGlobalSettings _globalSettings;

        public LuaStorageModule(LuaManager luaManager) : base(luaManager)
        {
            _globalSettings = SettingsProvider.Load<LuaGlobalSettings>();

            // Load profile values
            if (LuaManager.ProfileModel.LuaStorage != null)
                _profileValues = JsonToTable(LuaManager.ProfileModel.LuaStorage, LuaManager.LuaScript);
            else
                _profileValues = new Table(LuaManager.LuaScript);

            // Load global values
            if (_globalSettings.GlobalValues != null)
                _globalValues = JsonToTable(_globalSettings.GlobalValues, LuaManager.LuaScript);
            else
                _globalValues = new Table(LuaManager.LuaScript);

            // Set the values onto the globals table so scripters can access them
            LuaManager.LuaScript.Globals["ProfileStorage"] = _profileValues;
            LuaManager.LuaScript.Globals["GlobalStorage"] = _globalValues;
        }

        public override string ModuleName => null;

        public override void Dispose()
        {
            // Store profile values
            LuaManager.ProfileModel.LuaStorage = _profileValues.TableToJson();
            ProfileProvider.AddOrUpdate(LuaManager.ProfileModel);

            // Store global values
            _globalSettings.GlobalValues = _globalValues.TableToJson();
            _globalSettings.Save();
        }
    }

    public class LuaGlobalSettings : IArtemisSettings
    {
        public string GlobalValues { get; set; }

        public void Save()
        {
            SettingsProvider.Save(this);
        }

        public void Reset(bool save = false)
        {
            GlobalValues = null;

            if (save)
                Save();
        }
    }
}
