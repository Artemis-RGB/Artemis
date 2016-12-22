using System.Collections.Generic;
using System.Linq;

namespace Artemis.Profiles.Lua.Modules
{
    public class LuaProfileModule : LuaModule
    {
        private readonly ProfileModel _profileModel;

        public LuaProfileModule(LuaWrapper luaWrapper) : base(luaWrapper)
        {
            _profileModel = luaWrapper.ProfileModel;
        }

        public override string ModuleName => "Profile";

        #region General profile properties

        public string Name => _profileModel.Name;

        #endregion

        #region Overriding members

        public override void Dispose()
        {
        }

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