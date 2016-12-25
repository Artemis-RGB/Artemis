using System.Collections.Generic;
using System.Linq;
using Artemis.Managers;
using Artemis.Profiles.Lua.Wrappers;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaProfileModule : LuaModule
    {
        private readonly ProfileModel _profileModel;

        public LuaProfileModule(LuaManager luaManager) : base(luaManager)
        {
            _profileModel = luaManager.ProfileModel;
        }

        public override string ModuleName => "Profile";

        #region General profile properties

        public string Name => _profileModel.Name;

        #endregion

        #region Layer methods

        public List<LuaLayerWrapper> GetLayers()
        {
            return _profileModel.Layers.Select(l => new LuaLayerWrapper(l)).ToList();
        }

        public LuaLayerWrapper GetLayerByName(string name)
        {
            var layer = _profileModel.Layers.FirstOrDefault(l => l.Name == name);
            return layer == null ? null : new LuaLayerWrapper(layer);
        }

        #endregion
    }
}