using System.Collections.Generic;
using System.Linq;
using Artemis.Profiles.Layers.Models;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua
{
    /// <summary>
    ///     Serves as a sandboxed wrapper around the LayerModel
    /// </summary>
    [MoonSharpUserData]
    public class LuaLayerWrapper
    {
        private readonly LayerModel _layerModel;

        public LuaLayerWrapper(LayerModel layerModel)
        {
            _layerModel = layerModel;
        }

        #region Child methods

        public List<LuaLayerWrapper> GetChildren()
        {
            return _layerModel.Children.Select(l => new LuaLayerWrapper(l)).ToList();
        }

        public LuaLayerWrapper GetChildByName(string name)
        {
            var layer = _layerModel.Children.FirstOrDefault(l => l.Name == name);
            return layer == null ? null : new LuaLayerWrapper(layer);
        }

        #endregion

        #region General layer properties

        public string Name
        {
            get { return _layerModel.Name; }
            set { _layerModel.Name = value; }
        }

        public bool Enabled
        {
            get { return _layerModel.Enabled; }
            set { _layerModel.Enabled = value; }
        }

        public bool IsEvent
        {
            get { return _layerModel.IsEvent; }
            set { _layerModel.IsEvent = value; }
        }

        public LuaLayerWrapper Parent => new LuaLayerWrapper(_layerModel.Parent);

        #endregion

        #region Advanced layer properties

        public double X
        {
            get { return _layerModel.Properties.X; }
            set { _layerModel.Properties.X = value; }
        }

        public double Y
        {
            get { return _layerModel.Properties.Y; }
            set { _layerModel.Properties.Y = value; }
        }

        public double Width
        {
            get { return _layerModel.Properties.Width; }
            set { _layerModel.Properties.Width = value; }
        }

        public double Height
        {
            get { return _layerModel.Properties.Height; }
            set { _layerModel.Properties.Height = value; }
        }

        public bool Contain
        {
            get { return _layerModel.Properties.Contain; }
            set { _layerModel.Properties.Contain = value; }
        }

        public double Opacity
        {
            get { return _layerModel.Properties.Opacity; }
            set { _layerModel.Properties.Opacity = value; }
        }

        public double AnimationSpeed
        {
            get { return _layerModel.Properties.AnimationSpeed; }
            set { _layerModel.Properties.AnimationSpeed = value; }
        }

        public double AnimationProgress
        {
            get { return _layerModel.Properties.AnimationProgress; }
            set { _layerModel.Properties.AnimationProgress = value; }
        }

        #endregion
    }
}