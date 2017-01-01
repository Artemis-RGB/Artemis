using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Lua.Modules.Brushes;
using Artemis.Profiles.Lua.Wrappers;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaLayerModule : LuaModule
    {
        private readonly LayerModel _layerModel;

        public LuaLayerModule(LuaManager luaManager, LayerModel layerModel) : base(luaManager)
        {
            _layerModel = layerModel;
            SavedProperties = new Wrappers.LuaLayerProperties(_layerModel);

            // Trigger an update to fill up the Properties
            _layerModel.Update(new ProfilePreviewDataModel(), true, false);
        }

        public override string ModuleName => "Layer";

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

        #region Render layer properties

        public double X
        {
            get { return _layerModel.X; }
            set { _layerModel.X = value; }
        }

        public double Y
        {
            get { return _layerModel.Y; }
            set { _layerModel.Y = value; }
        }

        public double Width
        {
            get { return _layerModel.Width; }
            set { _layerModel.Width = value; }
        }

        public double Height
        {
            get { return _layerModel.Height; }
            set { _layerModel.Height = value; }
        }

        public double Opacity
        {
            get { return _layerModel.Opacity; }
            set { _layerModel.Opacity = value; }
        }

        public double AnimationProgress
        {
            get { return _layerModel.AnimationProgress; }
            set { _layerModel.AnimationProgress = value; }
        }

        #endregion

        #region Advanced layer properties

        public Wrappers.LuaLayerProperties SavedProperties { get; set; }

        public string BrushType => _layerModel.Properties.Brush?.GetType().Name;

        public LuaBrush Brush
        {
            get
            {
                if (_layerModel.Properties.Brush is SolidColorBrush)
                    return new LuaSolidColorBrush(_layerModel.Properties.Brush);
                if (_layerModel.Properties.Brush is LinearGradientBrush)
                    return new LuaLinearGradientBrush(_layerModel.Properties.Brush);
                if (_layerModel.Properties.Brush is RadialGradientBrush)
                    return new LuaRadialGradientBrush(_layerModel.Properties.Brush);
                return null;
            }
            set { _layerModel.Properties.Brush = value?.Brush; }
        }

        #endregion
    }

    [MoonSharpUserData]
    public class LuaLayerProperties
    {
        private readonly LayerModel _layerModel;

        public LuaLayerProperties(LayerModel layerModel)
        {
            _layerModel = layerModel;
        }

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
    }
}