using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua
{
    /// <summary>
    ///     Serves as a sandboxed wrapper around the ProfileModel
    /// </summary>
    [MoonSharpUserData]
    public class LuaProfileWrapper
    {
        private readonly ProfileModel _profileModel;

        public LuaProfileWrapper(ProfileModel profileModel)
        {
            _profileModel = profileModel;
        }

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