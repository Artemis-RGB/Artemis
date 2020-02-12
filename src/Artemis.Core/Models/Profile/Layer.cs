using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;
using Ninject;
using Ninject.Parameters;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Layer : ProfileElement
    {
        private readonly Dictionary<(Guid, string), BaseLayerProperty> _properties;
        private LayerShape _layerShape;
        private List<ArtemisLed> _leds;
        private SKPath _path;

        public Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;

            _leds = new List<ArtemisLed>();
            _properties = new Dictionary<(Guid, string), BaseLayerProperty>();

            CreateDefaultProperties();
            ApplyShapeType();

            ShapeTypeProperty.ValueChanged += (sender, args) => ApplyShapeType();
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
            _properties = new Dictionary<(Guid, string), BaseLayerProperty>();

            CreateDefaultProperties();
            ApplyShapeType();

            ShapeTypeProperty.ValueChanged += (sender, args) => ApplyShapeType();
        }

        internal LayerEntity LayerEntity { get; set; }

        /// <summary>
        ///     A collection of all the LEDs this layer is assigned to.
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();

        /// <summary>
        ///     A path containing all the LEDs this layer is applied to, any rendering outside the layer Path is clipped.
        ///     <para>For rendering, use the Path on <see cref="LayerShape" />.</para>
        /// </summary>
        public SKPath Path
        {
            get => _path;
            private set
            {
                _path = value;
                // I can't really be sure about the performance impact of calling Bounds often but
                // SkiaSharp calls SkiaApi.sk_path_get_bounds (Handle, &rect); which sounds expensive
                Bounds = value?.Bounds ?? SKRect.Empty;
            }
        }

        /// <summary>
        ///     The bounds of this layer
        /// </summary>
        public SKRect Bounds { get; private set; }

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

        public LayerProperty<LayerShapeType> ShapeTypeProperty { get; set; }

        public LayerProperty<LayerFillType> FillTypeProperty { get; set; }

        public LayerProperty<SKBlendMode> BlendModeProperty { get; set; }

        public LayerProperty<LayerBrushReference> BrushReferenceProperty { get; set; }

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
        public LayerProperty<SKSize> ScaleProperty { get; private set; }

        /// <summary>
        ///     The rotation property of this layer range 0 - 360, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<float> RotationProperty { get; private set; }

        /// <summary>
        ///     The opacity property of this layer range 0 - 100, also found in <see cref="Properties" />
        /// </summary>
        public LayerProperty<float> OpacityProperty { get; private set; }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape" />.
        /// </summary>
        public LayerBrush LayerBrush { get; internal set; }

        #region Storage

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
        }

        #endregion

        #region Rendering

        public override void Update(double deltaTime)
        {
            foreach (var property in Properties)
                property.KeyframeEngine?.Update(deltaTime);

            // For now, reset all keyframe engines after the last keyframe was hit
            // This is a placeholder method of repeating the animation until repeat modes are implemented
            var lastKeyframe = Properties.SelectMany(p => p.UntypedKeyframes).OrderByDescending(t => t.Position).FirstOrDefault();
            if (lastKeyframe != null)
            {
                if (Properties.Any(p => p.KeyframeEngine?.Progress > lastKeyframe.Position))
                {
                    foreach (var baseLayerProperty in Properties)
                        baseLayerProperty.KeyframeEngine?.OverrideProgress(TimeSpan.Zero);
                }
            }

            LayerBrush?.Update(deltaTime);
        }

        public override void Render(double deltaTime, SKCanvas canvas)
        {
            if (Path == null || LayerShape?.Path == null)
                return;

            canvas.Save();
            canvas.ClipPath(Path);

            using (var paint = new SKPaint())
            {
                paint.BlendMode = BlendModeProperty.CurrentValue;
                paint.Color = new SKColor(0, 0, 0, (byte) (OpacityProperty.CurrentValue * 2.55f));

                switch (FillTypeProperty.CurrentValue)
                {
                    case LayerFillType.Stretch:
                        StretchRender(canvas, paint);
                        break;
                    case LayerFillType.Clip:
                        ClipRender(canvas, paint);
                        break;
                    case LayerFillType.Tile:
                        TileRender(canvas, paint);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            canvas.Restore();
        }

        private void StretchRender(SKCanvas canvas, SKPaint paint)
        {
            // Apply transformations
            var sizeProperty = ScaleProperty.CurrentValue;
            var rotationProperty = RotationProperty.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = AnchorPointProperty.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            // Apply these before translation because anchorPosition takes translation into account
            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
            canvas.Scale(sizeProperty.Width, sizeProperty.Height, anchorPosition.X, anchorPosition.Y);
            // Once the other transformations are done it is save to translate
            canvas.Translate(x, y);

            LayerBrush?.Render(canvas, LayerShape.Path, paint);
        }

        private void ClipRender(SKCanvas canvas, SKPaint paint)
        {
            // Apply transformations
            var sizeProperty = ScaleProperty.CurrentValue;
            var rotationProperty = RotationProperty.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = AnchorPointProperty.CurrentValue;

            // Translation originates from the unscaled center of the layer and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            // Rotation is always applied on the canvas
            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);

            var path = new SKPath(LayerShape.Path);
            path.Transform(SKMatrix.MakeTranslation(x, y));
            path.Transform(SKMatrix.MakeScale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y));

            LayerBrush?.Render(canvas, path, paint);
        }

        private void TileRender(SKCanvas canvas, SKPaint paint)
        {
            // TODO
            // Apply transformations
            var sizeProperty = ScaleProperty.CurrentValue;
            var rotationProperty = RotationProperty.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = AnchorPointProperty.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            // Apply these before translation because anchorPosition takes translation into account
            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
            canvas.Scale(sizeProperty.Width, sizeProperty.Height, anchorPosition.X, anchorPosition.Y);
            // Once the other transformations are done it is save to translate
            canvas.Translate(x, y);

            LayerBrush?.Render(canvas, LayerShape.Path, paint);
        }

        internal void CalculateRenderProperties()
        {
            if (!Leds.Any())
            {
                Path = new SKPath();

                LayerShape?.CalculateRenderProperties();
                OnRenderPropertiesUpdated();
                return;
            }

            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRenderRectangle);

            Path = path;

            // This is called here so that the shape's render properties are up to date when other code
            // responds to OnRenderPropertiesUpdated
            LayerShape?.CalculateRenderProperties();
            OnRenderPropertiesUpdated();
        }

        private SKPoint GetLayerAnchorPosition()
        {
            var positionProperty = PositionProperty.CurrentValue;

            // Start at the center of the shape
            var position = new SKPoint(Bounds.MidX, Bounds.MidY);

            // Apply translation
            position.X += positionProperty.X * Bounds.Width;
            position.Y += positionProperty.Y * Bounds.Height;

            return position;
        }

        #endregion

        #region LED management

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

        #endregion

        #region Shape management

        private void ApplyShapeType()
        {
            switch (ShapeTypeProperty.CurrentValue)
            {
                case LayerShapeType.Ellipse:
                    LayerShape = new Ellipse(this);
                    break;
                case LayerShapeType.Rectangle:
                    LayerShape = new Rectangle(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Adds the provided layer property and its children to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <typeparam name="T">The type of value of the layer property</typeparam>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        internal bool RegisterLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            return RegisterLayerProperty((BaseLayerProperty) layerProperty);
        }

        /// <summary>
        ///     Adds the provided layer property to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        internal bool RegisterLayerProperty(BaseLayerProperty layerProperty)
        {
            if (_properties.ContainsKey((layerProperty.PluginInfo.Guid, layerProperty.Id)))
                throw new ArtemisCoreException($"Duplicate property ID detected. Layer already contains a property with ID {layerProperty.Id}.");

            var propertyEntity = LayerEntity.PropertyEntities.FirstOrDefault(p => p.Id == layerProperty.Id && p.ValueType == layerProperty.Type.Name);
            // TODO: Catch serialization exceptions and log them
            if (propertyEntity != null)
                layerProperty.ApplyToProperty(propertyEntity);

            _properties.Add((layerProperty.PluginInfo.Guid, layerProperty.Id), layerProperty);
            OnLayerPropertyRegistered(new LayerPropertyEventArgs(layerProperty));
            return propertyEntity != null;
        }

        /// <summary>
        ///     Removes the provided layer property from the layer.
        /// </summary>
        /// <typeparam name="T">The type of value of the layer property</typeparam>
        /// <param name="layerProperty">The property to remove from the layer</param>
        public void RemoveLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            RemoveLayerProperty((BaseLayerProperty) layerProperty);
        }

        /// <summary>
        ///     Removes the provided layer property from the layer.
        /// </summary>
        /// <param name="layerProperty">The property to remove from the layer</param>
        public void RemoveLayerProperty(BaseLayerProperty layerProperty)
        {
            if (!_properties.ContainsKey((layerProperty.PluginInfo.Guid, layerProperty.Id)))
                throw new ArtemisCoreException($"Could not find a property with ID {layerProperty.Id}.");

            var property = _properties[(layerProperty.PluginInfo.Guid, layerProperty.Id)];
            property.Parent?.Children.Remove(property);
            _properties.Remove((layerProperty.PluginInfo.Guid, layerProperty.Id));

            OnLayerPropertyRemoved(new LayerPropertyEventArgs(property));
        }

        /// <summary>
        ///     If found, returns the <see cref="LayerProperty{T}" /> matching the provided ID
        /// </summary>
        /// <typeparam name="T">The type of the layer property</typeparam>
        /// <param name="pluginInfo">The plugin this property belongs to</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public LayerProperty<T> GetLayerPropertyById<T>(PluginInfo pluginInfo, string id)
        {
            if (!_properties.ContainsKey((pluginInfo.Guid, id)))
                return null;

            var property = _properties[(pluginInfo.Guid, id)];
            if (property.Type != typeof(T))
                throw new ArtemisCoreException($"Property type mismatch. Expected property {property} to have type {typeof(T)} but it has {property.Type} instead.");
            return (LayerProperty<T>) _properties[(pluginInfo.Guid, id)];
        }

        private void CreateDefaultProperties()
        {
            // Shape
            var shape = new LayerProperty<object>(this, "Core.Shape", "Shape", "A collection of basic shape properties.");
            ShapeTypeProperty = new LayerProperty<LayerShapeType>(this, shape, "Core.ShapeType", "Shape type", "The type of shape to draw in this layer.") {CanUseKeyframes = false};
            FillTypeProperty = new LayerProperty<LayerFillType>(this, shape, "Core.FillType", "Fill type", "How to make the shape adjust to scale changes.") {CanUseKeyframes = false};
            BlendModeProperty = new LayerProperty<SKBlendMode>(this, shape, "Core.BlendMode", "Blend mode", "How to blend this layer into the resulting image.") {CanUseKeyframes = false};
            ShapeTypeProperty.Value = LayerShapeType.Rectangle;
            FillTypeProperty.Value = LayerFillType.Stretch;
            BlendModeProperty.Value = SKBlendMode.SrcOver;

            RegisterLayerProperty(shape);
            foreach (var shapeProperty in shape.Children)
                RegisterLayerProperty(shapeProperty);

            // Brush
            var brush = new LayerProperty<object>(this, "Core.Brush", "Brush", "A collection of properties that configure the selected brush.");
            BrushReferenceProperty = new LayerProperty<LayerBrushReference>(this, brush, "Core.BrushReference", "Brush type", "The type of brush to use for this layer.") {CanUseKeyframes = false};

            RegisterLayerProperty(brush);
            foreach (var brushProperty in brush.Children)
                RegisterLayerProperty(brushProperty);

            // Transform
            var transform = new LayerProperty<object>(this, "Core.Transform", "Transform", "A collection of transformation properties.") {ExpandByDefault = true};
            AnchorPointProperty = new LayerProperty<SKPoint>(this, transform, "Core.AnchorPoint", "Anchor Point", "The point at which the shape is attached to its position.");
            PositionProperty = new LayerProperty<SKPoint>(this, transform, "Core.Position", "Position", "The position of the shape.");
            ScaleProperty = new LayerProperty<SKSize>(this, transform, "Core.Scale", "Scale", "The scale of the shape.") {InputAffix = "%"};
            RotationProperty = new LayerProperty<float>(this, transform, "Core.Rotation", "Rotation", "The rotation of the shape in degrees.") {InputAffix = "°"};
            OpacityProperty = new LayerProperty<float>(this, transform, "Core.Opacity", "Opacity", "The opacity of the shape.") {InputAffix = "%"};
            ScaleProperty.Value = new SKSize(100, 100);
            OpacityProperty.Value = 100;

            RegisterLayerProperty(transform);
            foreach (var transformProperty in transform.Children)
                RegisterLayerProperty(transformProperty);
        }

        #endregion

        #region Events

        public event EventHandler RenderPropertiesUpdated;
        public event EventHandler ShapePropertiesUpdated;
        public event EventHandler<LayerPropertyEventArgs> LayerPropertyRegistered;
        public event EventHandler<LayerPropertyEventArgs> LayerPropertyRemoved;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnShapePropertiesUpdated()
        {
            ShapePropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        private void OnLayerPropertyRegistered(LayerPropertyEventArgs e)
        {
            LayerPropertyRegistered?.Invoke(this, e);
        }

        private void OnLayerPropertyRemoved(LayerPropertyEventArgs e)
        {
            LayerPropertyRemoved?.Invoke(this, e);
        }
    }

    public enum LayerShapeType
    {
        Ellipse,
        Rectangle
    }

    public enum LayerFillType
    {
        Stretch,
        Clip,
        Tile
    }
}