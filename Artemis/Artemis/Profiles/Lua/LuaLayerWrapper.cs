using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Lua.Brushes;
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

            // Triger an update to fill up the AppliedProperties
            _layerModel.Update(new ProfilePreviewDataModel(), true, false);
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
            get { return _layerModel.AppliedProperties.X; }
            set { _layerModel.AppliedProperties.X = value; }
        }

        public double Y
        {
            get { return _layerModel.AppliedProperties.Y; }
            set { _layerModel.AppliedProperties.Y = value; }
        }

        public double Width
        {
            get { return _layerModel.AppliedProperties.Width; }
            set { _layerModel.AppliedProperties.Width = value; }
        }

        public double Height
        {
            get { return _layerModel.AppliedProperties.Height; }
            set { _layerModel.AppliedProperties.Height = value; }
        }

        public bool Contain
        {
            get { return _layerModel.AppliedProperties.Contain; }
            set { _layerModel.AppliedProperties.Contain = value; }
        }

        public double Opacity
        {
            get { return _layerModel.AppliedProperties.Opacity; }
            set { _layerModel.AppliedProperties.Opacity = value; }
        }

        public double AnimationSpeed
        {
            get { return _layerModel.AppliedProperties.AnimationSpeed; }
            set { _layerModel.AppliedProperties.AnimationSpeed = value; }
        }

        public double AnimationProgress
        {
            get { return _layerModel.AppliedProperties.AnimationProgress; }
            set { _layerModel.AppliedProperties.AnimationProgress = value; }
        }

        public string BrushType => _layerModel.AppliedProperties.Brush?.GetType().Name;

        public LuaBrush Brush
        {
            get
            {
                if (_layerModel.AppliedProperties.Brush is SolidColorBrush)
                    return new LuaSolidColorBrush(_layerModel.AppliedProperties.Brush);
                if (_layerModel.AppliedProperties.Brush is LinearGradientBrush)
                    return new LuaLinearGradientBrush(_layerModel.AppliedProperties.Brush);
                if (_layerModel.AppliedProperties.Brush is RadialGradientBrush)
                    return new LuaRadialGradientBrush(_layerModel.AppliedProperties.Brush);
                return null;
            }
            set { _layerModel.AppliedProperties.Brush = value?.Brush; }
        }

        #endregion
    }
}