using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Layer : ProfileElement
    {
        private LayerShape _layerShape;
        private List<ArtemisLed> _leds;

        public Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;

            _leds = new List<ArtemisLed>();
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Order = layerEntity.Order;

            switch (layerEntity.ShapeEntity?.Type)
            {
                case ShapeEntityType.Ellipse:
                    LayerShape = new Ellipse(this, layerEntity.ShapeEntity);
                    break;
                case ShapeEntityType.Fill:
                    LayerShape = new Fill(this, layerEntity.ShapeEntity);
                    break;
                case ShapeEntityType.Polygon:
                    LayerShape = new Polygon(this, layerEntity.ShapeEntity);
                    break;
                case ShapeEntityType.Rectangle:
                    LayerShape = new Rectangle(this, layerEntity.ShapeEntity);
                    break;
                case null:
                    LayerShape = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _leds = new List<ArtemisLed>();
        }

        internal LayerEntity LayerEntity { get; set; }

        /// <summary>
        ///     A collection of all the LEDs this layer is assigned to.
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();

        /// <summary>
        ///     A rectangle relative to the surface that contains all the LEDs in this layer.
        ///     <para>For rendering, use the RenderRectangle on <see cref="LayerShape" />.</para>
        /// </summary>
        public SKRect Rectangle { get; private set; }

        /// <summary>
        ///     A zero-based absolute rectangle that contains all the LEDs in this layer.
        ///     <para>For rendering, use the RenderRectangle on <see cref="LayerShape" />.</para>
        /// </summary>
        public SKRect AbsoluteRectangle { get; private set; }

        /// <summary>
        ///     A path containing all the LEDs this layer is applied to.
        ///     <para>For rendering, use the RenderPath on <see cref="LayerShape" />.</para>
        /// </summary>
        public SKPath Path { get; private set; }

        /// <summary>
        ///     Defines the shape that is rendered by the <see cref="LayerBrush"/>.
        /// </summary>
        public LayerShape LayerShape
        {
            get => _layerShape;
            set
            {
                _layerShape = value;
                if (Path != null)
                    CalculateRenderProperties();
            }
        }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape"/>.
        /// </summary>
        public LayerBrush LayerBrush { get; internal set; }

        public override void Update(double deltaTime)
        {
            LayerBrush?.Update(deltaTime);
        }

        public override void Render(double deltaTime, SKCanvas canvas)
        {
            if (Path == null)
                return;

            canvas.Save();
            LayerBrush?.Render(canvas);
            canvas.Restore();
        }

        internal override void ApplyToEntity()
        {
            // Properties
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();
            LayerEntity.Order = Order;
            LayerEntity.Name = Name;
            LayerEntity.ProfileId = Profile.EntityId;

            // LEDs
            LayerEntity.Leds.Clear();
            foreach (var artemisLed in Leds)
            {
                var ledEntity = new LedEntity
                {
                    DeviceHash = artemisLed.Device.RgbDevice.GetDeviceHashCode(),
                    LedName = artemisLed.RgbLed.Id.ToString()
                };
                LayerEntity.Leds.Add(ledEntity);
            }

            // Conditions TODO
            LayerEntity.Condition.Clear();

            // Brush
            if (LayerBrush != null)
            {
                LayerEntity.BrushEntity = new BrushEntity
                {
                    BrushPluginGuid = LayerBrush.Descriptor.LayerBrushProvider.PluginInfo.Guid,
                    BrushType = LayerBrush.GetType().Name,
                    Configuration = JsonConvert.SerializeObject(LayerBrush.Settings)
                };
            }

            // Shape
            LayerShape?.ApplyToEntity();
        }

        /// <summary>
        ///     Adds a new <see cref="ArtemisLed" /> to the layer and updates the render properties.
        /// </summary>
        /// <param name="led">The LED to add</param>
        public void AddLed(ArtemisLed led)
        {
            _leds.Add(led);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Adds a collection of new <see cref="ArtemisLed" />s to the layer and updates the render properties.
        /// </summary>
        /// <param name="leds">The LEDs to add</param>
        public void AddLeds(IEnumerable<ArtemisLed> leds)
        {
            _leds.AddRange(leds);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Removes a <see cref="ArtemisLed" /> from the layer and updates the render properties.
        /// </summary>
        /// <param name="led">The LED to remove</param>
        public void RemoveLed(ArtemisLed led)
        {
            _leds.Remove(led);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Removes all <see cref="ArtemisLed" />s from the layer and updates the render properties.
        /// </summary>
        public void ClearLeds()
        {
            _leds.Clear();
            CalculateRenderProperties();
        }

        internal void PopulateLeds(ArtemisSurface surface)
        {
            var leds = new List<ArtemisLed>();

            // Get the surface LEDs for this layer
            var availableLeds = surface.Devices.SelectMany(d => d.Leds).ToList();
            foreach (var ledEntity in LayerEntity.Leds)
            {
                var match = availableLeds.FirstOrDefault(a => a.Device.RgbDevice.GetDeviceHashCode() == ledEntity.DeviceHash &&
                                                              a.RgbLed.Id.ToString() == ledEntity.LedName);
                if (match != null)
                    leds.Add(match);
            }

            _leds = leds;
            CalculateRenderProperties();
        }

        internal void CalculateRenderProperties()
        {
            if (!Leds.Any())
            {
                Rectangle = SKRect.Empty;
                AbsoluteRectangle = SKRect.Empty;
                Path = new SKPath();
                OnRenderPropertiesUpdated();
                return;
            }

            // Determine to top-left and bottom-right
            var minX = Leds.Min(l => l.AbsoluteRenderRectangle.Left);
            var minY = Leds.Min(l => l.AbsoluteRenderRectangle.Top);
            var maxX = Leds.Max(l => l.AbsoluteRenderRectangle.Right);
            var maxY = Leds.Max(l => l.AbsoluteRenderRectangle.Bottom);

            Rectangle = SKRect.Create(minX, minY, maxX - minX, maxY - minY);
            AbsoluteRectangle = SKRect.Create(0, 0, maxX - minX, maxY - minY);

            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRenderRectangle);

            Path = path;
            LayerShape.CalculateRenderProperties();
            OnRenderPropertiesUpdated();
        }

        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        #region Events

        public event EventHandler RenderPropertiesUpdated;
        public event EventHandler ShapePropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnShapePropertiesUpdated()
        {
            ShapePropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}