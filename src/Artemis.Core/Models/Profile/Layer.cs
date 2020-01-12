using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.LayerProperties;
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
        private readonly Dictionary<string, BaseLayerProperty> _properties;
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
            _properties = new Dictionary<string, BaseLayerProperty>();

            CreateDefaultProperties();
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Order = layerEntity.Order;

            _leds = new List<ArtemisLed>();
            _properties = new Dictionary<string, BaseLayerProperty>();

            CreateDefaultProperties();

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
        ///     Defines the shape that is rendered by the <see cref="LayerBrush" />.
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
        ///     A collection of all the properties on this layer
        /// </summary>
        public ReadOnlyCollection<BaseLayerProperty> Properties => _properties.Values.ToList().AsReadOnly();

        /// <summary>
        ///     The anchor point property of this layer, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<SKPoint> AnchorPointProperty { get; private set; }

        /// <summary>
        ///     The position of this layer, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<SKPoint> PositionProperty { get; private set; }

        /// <summary>
        ///     The size property of this layer, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<SKSize> SizeProperty { get; private set; }

        /// <summary>
        ///     The rotation property of this layer, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<int> RotationProperty { get; private set; }

        /// <summary>
        ///     The opacity property of this layer, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<float> OpacityProperty { get; private set; }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape" />.
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
            canvas.ClipPath(Path);
            // Placeholder
            if (LayerShape?.RenderPath != null)
                canvas.DrawPath(LayerShape.RenderPath, new SKPaint {Color = new SKColor(255, 0, 0)});
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
            foreach (var layerProperty in Properties)
                layerProperty.ApplyToEntity();

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
            // This is called here so that the shape's render properties are up to date when other code
            // responds to OnRenderPropertiesUpdated
            LayerShape?.CalculateRenderProperties(PositionProperty.GetCurrentValue(), SizeProperty.GetCurrentValue());

            OnRenderPropertiesUpdated();
        }


        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        #region Properties

        /// <summary>
        ///     Adds the provided layer property to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <typeparam name="T">The type of value of the layer property</typeparam>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        public bool AddLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            return AddLayerProperty((BaseLayerProperty) layerProperty);
        }

        /// <summary>
        ///     Adds the provided layer property to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        public bool AddLayerProperty(BaseLayerProperty layerProperty)
        {
            if (_properties.ContainsKey(layerProperty.Id))
                throw new ArtemisCoreException($"Duplicate property ID detected. Layer already contains a property with ID {layerProperty.Id}.");

            var propertyEntity = LayerEntity.PropertyEntities.FirstOrDefault(p => p.Id == layerProperty.Id && p.ValueType == layerProperty.Type.Name);
            // TODO: Catch serialization exceptions and log them
            if (propertyEntity != null) 
                layerProperty.ApplyToProperty(propertyEntity);

            _properties.Add(layerProperty.Id, layerProperty);
            return propertyEntity != null;
        }

        /// <summary>
        ///     If found, returns the <see cref="LayerProperty{T}" /> matching the provided ID
        /// </summary>
        /// <typeparam name="T">The type of the layer property</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public LayerProperty<T> GetLayerPropertyById<T>(string id)
        {
            if (!_properties.ContainsKey(id))
                return null;

            var property = _properties[id];
            if (property.Type != typeof(T))
                throw new ArtemisCoreException($"Property type mismatch. Expected property {property} to have type {typeof(T)} but it has {property.Type} instead.");
            return (LayerProperty<T>) _properties[id];
        }

        private void CreateDefaultProperties()
        {
            var transformProperty = new LayerProperty<object>(this, null, "Core.Transform", "Transform", "The default properties collection every layer has, allows you to transform the shape.") {ExpandByDefault = true};
            AnchorPointProperty = new LayerProperty<SKPoint>(this, transformProperty, "Core.AnchorPoint", "Anchor Point", "The point at which the shape is attached to its position.");
            PositionProperty = new LayerProperty<SKPoint>(this, transformProperty, "Core.Position", "Position", "The position of the shape.");
            SizeProperty = new LayerProperty<SKSize>(this, transformProperty, "Core.Size", "Size", "The size of the shape.") {InputAffix = "%"};
            RotationProperty = new LayerProperty<int>(this, transformProperty, "Core.Rotation", "Rotation", "The rotation of the shape in degrees.") {InputAffix = "°"};
            OpacityProperty = new LayerProperty<float>(this, transformProperty, "Core.Opacity", "Opacity", "The opacity of the shape.") {InputAffix = "%"};
            transformProperty.Children.Add(AnchorPointProperty);
            transformProperty.Children.Add(PositionProperty);
            transformProperty.Children.Add(SizeProperty);
            transformProperty.Children.Add(RotationProperty);
            transformProperty.Children.Add(OpacityProperty);

            AddLayerProperty(transformProperty);
            foreach (var transformPropertyChild in transformProperty.Children)
                AddLayerProperty(transformPropertyChild);
        }

        #endregion

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