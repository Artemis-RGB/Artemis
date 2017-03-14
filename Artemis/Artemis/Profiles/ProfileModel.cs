using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.ParentChild;
using Newtonsoft.Json;

namespace Artemis.Profiles
{
    public class ProfileModel
    {
        private readonly char[] _invalidFileNameChars;
        private List<KeybindModel> _profileBinds;

        public ProfileModel()
        {
            _invalidFileNameChars = Path.GetInvalidFileNameChars();
            _profileBinds = new List<KeybindModel>();

            Layers = new ChildItemCollection<ProfileModel, LayerModel>(this);
            OnProfileUpdatedEvent += OnOnProfileUpdatedEvent;
        }

        public ChildItemCollection<ProfileModel, LayerModel> Layers { get; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string KeyboardSlug { get; set; }
        public string GameName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string LuaScript { get; set; }

        [JsonIgnore]
        public string Slug => new string(Name.Where(ch => !_invalidFileNameChars.Contains(ch)).ToArray());

        private void OnOnProfileUpdatedEvent(object sender, EventArgs e)
        {
            ClearKeybinds();
            ApplyKeybinds();
        }

        public event EventHandler<ProfileDeviceEventsArg> OnDeviceUpdatedEvent;
        public event EventHandler<ProfileDeviceEventsArg> OnDeviceDrawnEvent;
        public event EventHandler<EventArgs> OnProfileUpdatedEvent;

        public void FixOrder()
        {
            Layers.Sort(l => l.Order);
            for (var i = 0; i < Layers.Count; i++)
                Layers[i].Order = i;
        }

        /// <summary>
        ///     Gives all the layers and their children in a flat list
        /// </summary>
        public List<LayerModel> GetLayers()
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers.OrderBy(l => l.Order))
            {
                layers.Add(layerModel);
                layers.AddRange(layerModel.GetLayers());
            }

            return layers;
        }

        /// <summary>
        ///     Generates a flat list containing all layers that must be rendered on the keyboard,
        ///     the first mouse layer to be rendered and the first headset layer to be rendered
        /// </summary>
        /// <param name="dataModel">Instance of said game data model</param>
        /// <param name="keyboardOnly">Whether or not to ignore anything but keyboards</param>
        /// <param name="ignoreConditions"></param>
        /// <returns>A flat list containing all layers that must be rendered</returns>
        public List<LayerModel> GetRenderLayers(ModuleDataModel dataModel, bool keyboardOnly,
            bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers.OrderByDescending(l => l.Order))
            {
                if (!layerModel.Enabled || keyboardOnly && layerModel.LayerType.DrawType != DrawType.Keyboard)
                    continue;

                if (!ignoreConditions)
                    if (!layerModel.AreConditionsMet(dataModel))
                        continue;

                layers.Add(layerModel);
                layers.AddRange(layerModel.GetRenderLayers(dataModel, keyboardOnly, ignoreConditions));
            }

            return layers;
        }

        /// <summary>
        ///     Draw all the given layers on the given rect
        /// </summary>
        /// <param name="deviceVisualModel"></param>
        /// <param name="renderLayers">The layers to render</param>
        /// <param name="dataModel">The data model to base the layer's properties on</param>
        /// <param name="preview">Indicates wheter the layer is drawn as a preview, ignoring dynamic properties</param>
        internal void DrawLayers(DeviceVisualModel deviceVisualModel, List<LayerModel> renderLayers,
            ModuleDataModel dataModel, bool preview)
        {
            renderLayers = renderLayers.Where(rl => rl.LayerType.DrawType == deviceVisualModel.DrawType).ToList();
            if (!renderLayers.Any())
                return;

            // Setup the DrawingVisual's size
            var c = deviceVisualModel.GetDrawingContext();

            c.PushClip(new RectangleGeometry(deviceVisualModel.Rect));
            c.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, deviceVisualModel.Rect);

            // Update the layers
            foreach (var layerModel in renderLayers)
                layerModel.Update(dataModel, preview, true);
            RaiseDeviceUpdatedEvent(new ProfileDeviceEventsArg(deviceVisualModel.DrawType, dataModel, preview, null));

            // Draw the layers
            foreach (var layerModel in renderLayers)
                layerModel.Draw(dataModel, c, preview, true);
            RaiseDeviceDrawnEvent(new ProfileDeviceEventsArg(deviceVisualModel.DrawType, dataModel, preview, c));

            // Remove the clip
            c.Pop();
        }

        private void RaiseDeviceUpdatedEvent(ProfileDeviceEventsArg e)
        {
            OnDeviceUpdatedEvent?.Invoke(this, e);
        }

        public void RaiseDeviceDrawnEvent(ProfileDeviceEventsArg e)
        {
            OnDeviceDrawnEvent?.Invoke(this, e);
        }

        public virtual void OnOnProfileUpdatedEvent()
        {
            OnProfileUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Looks at all the layers wthin the profile and makes sure they are within boundaries of the given rectangle
        /// </summary>
        /// <param name="keyboardRectangle"></param>
        public void FixBoundaries(Rect keyboardRectangle)
        {
            foreach (var layer in GetLayers())
            {
                if (!layer.LayerType.ShowInEdtor)
                    continue;

                var props = layer.Properties;
                var layerRect = new Rect(new Point(props.X, props.Y), new Size(props.Width, props.Height));
                if (keyboardRectangle.Contains(layerRect))
                    continue;

                props.X = 0;
                props.Y = 0;
                layer.Properties = props;
            }
        }

        /// <summary>
        ///     Resizes layers that are shown in the editor and match exactly the full keyboard widht and height
        /// </summary>
        /// <param name="target">The new keyboard to adjust the layers for</param>
        public void ResizeLayers(KeyboardProvider target)
        {
            foreach (var layer in GetLayers())
            {
                if (!layer.LayerType.ShowInEdtor ||
                    !(Math.Abs(layer.Properties.Width - Width) < 0.01) ||
                    !(Math.Abs(layer.Properties.Height - Height) < 0.01))
                    continue;

                layer.Properties.Width = target.Width;
                layer.Properties.Height = target.Height;
            }
        }

        public void Activate(LuaManager luaManager)
        {
            ApplyKeybinds();
            luaManager.SetupLua(this);
        }

        public void Deactivate(LuaManager luaManager)
        {
            ClearKeybinds();
            luaManager.ClearLua();
        }

        public LayerModel AddLayer(LayerModel afterLayer)
        {
            // Create a new layer
            var layer = LayerModel.CreateLayer();

            if (afterLayer != null)
            {
                afterLayer.InsertAfter(layer);
            }
            else
            {
                Layers.Add(layer);
                FixOrder();
            }

            return layer;
        }

        public void ApplyKeybinds()
        {
            foreach (var layerModel in GetLayers())
                layerModel.SetupKeybinds();
        }

        public void ClearKeybinds()
        {
            foreach (var layerModel in GetLayers())
                layerModel.RemoveKeybinds();
        }

        #region Compare

        protected bool Equals(ProfileModel other)
        {
            return string.Equals(Slug, other.Slug) &&
                   string.Equals(KeyboardSlug, other.KeyboardSlug) &&
                   string.Equals(GameName, other.GameName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((ProfileModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Slug?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (KeyboardSlug?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (GameName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion
    }
}
