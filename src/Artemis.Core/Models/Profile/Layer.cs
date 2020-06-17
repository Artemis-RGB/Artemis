using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Annotations;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    /// <summary>
    ///     Represents a layer on a profile. To create new layers use the <see cref="LayerService" /> by injecting
    ///     <see cref="ILayerService" /> into your code
    /// </summary>
    public sealed class Layer : EffectProfileElement
    {
        private LayerShape _layerShape;
        private List<ArtemisLed> _leds;

        internal Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            General = new LayerGeneralProperties {IsCorePropertyGroup = true};
            Transform = new LayerTransformProperties {IsCorePropertyGroup = true};

            _layerEffects = new List<BaseLayerEffect>();
            _leds = new List<ArtemisLed>();
            _expandedPropertyGroups = new List<string>();

            General.PropertyGroupInitialized += GeneralOnPropertyGroupInitialized;
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Order = layerEntity.Order;
            General = new LayerGeneralProperties {IsCorePropertyGroup = true};
            Transform = new LayerTransformProperties {IsCorePropertyGroup = true};

            _layerEffects = new List<BaseLayerEffect>();
            _leds = new List<ArtemisLed>();
            _expandedPropertyGroups = new List<string>();
            _expandedPropertyGroups.AddRange(layerEntity.ExpandedPropertyGroups);

            General.PropertyGroupInitialized += GeneralOnPropertyGroupInitialized;
        }

        internal LayerEntity LayerEntity { get; set; }
        internal override PropertiesEntity PropertiesEntity => LayerEntity;
        internal override EffectsEntity EffectsEntity => LayerEntity;

        /// <summary>
        ///     A collection of all the LEDs this layer is assigned to.
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();

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

        [PropertyGroupDescription(Name = "General", Description = "A collection of general properties")]
        public LayerGeneralProperties General { get; set; }

        [PropertyGroupDescription(Name = "Transform", Description = "A collection of transformation properties")]
        public LayerTransformProperties Transform { get; set; }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape" />.
        /// </summary>
        public BaseLayerBrush LayerBrush { get; internal set; }

        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        #region Storage

        internal override void ApplyToEntity()
        {
            // Properties
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();
            LayerEntity.Order = Order;
            LayerEntity.Name = Name;
            LayerEntity.ProfileId = Profile.EntityId;
            LayerEntity.ExpandedPropertyGroups.Clear();
            LayerEntity.ExpandedPropertyGroups.AddRange(_expandedPropertyGroups);

            General.ApplyToEntity();
            Transform.ApplyToEntity();
            LayerBrush?.BaseProperties.ApplyToEntity();

            // Effects
            ApplyLayerEffectsToEntity();

            // LEDs
            LayerEntity.Leds.Clear();
            foreach (var artemisLed in Leds)
            {
                var ledEntity = new LedEntity
                {
                    DeviceIdentifier = artemisLed.Device.RgbDevice.GetDeviceIdentifier(),
                    LedName = artemisLed.RgbLed.Id.ToString()
                };
                LayerEntity.Leds.Add(ledEntity);
            }

            // Conditions TODO
            LayerEntity.Conditions.Clear();
        }

        #endregion

        #region Shape management

        private void GeneralOnPropertyGroupInitialized(object sender, EventArgs e)
        {
            ApplyShapeType();
            General.ShapeType.BaseValueChanged -= ShapeTypeOnBaseValueChanged;
            General.ShapeType.BaseValueChanged += ShapeTypeOnBaseValueChanged;
        }

        private void ShapeTypeOnBaseValueChanged(object sender, EventArgs e)
        {
            ApplyShapeType();
        }

        private void ApplyShapeType()
        {
            switch (General.ShapeType.CurrentValue)
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

        #region Rendering

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
            if (LayerBrush?.BaseProperties == null || !LayerBrush.BaseProperties.PropertiesInitialized)
                return;

            // TODO: Remove, this is slow and stupid
            // For now, reset all keyframe engines after the last keyframe was hit
            // This is a placeholder method of repeating the animation until repeat modes are implemented
            var properties = new List<BaseLayerProperty>(General.GetAllLayerProperties().Where(p => p.BaseKeyframes.Any()));
            properties.AddRange(Transform.GetAllLayerProperties().Where(p => p.BaseKeyframes.Any()));
            properties.AddRange(LayerBrush.BaseProperties.GetAllLayerProperties().Where(p => p.BaseKeyframes.Any()));
            var timeLineEnd = properties.Any() ? properties.Max(p => p.BaseKeyframes.Max(k => k.Position)) : TimeSpan.MaxValue;
            if (properties.Any(p => p.TimelineProgress >= timeLineEnd))
            {
                General.Override(TimeSpan.Zero);
                Transform.Override(TimeSpan.Zero);
                LayerBrush.BaseProperties.Override(TimeSpan.Zero);
                foreach (var baseLayerEffect in LayerEffects)
                    baseLayerEffect.BaseProperties?.Override(TimeSpan.Zero);
            }
            else
            {
                General.Update(deltaTime);
                Transform.Update(deltaTime);
                LayerBrush.BaseProperties.Update(deltaTime);
                foreach (var baseLayerEffect in LayerEffects)
                    baseLayerEffect.BaseProperties?.Update(deltaTime);
            }

            LayerBrush.Update(deltaTime);
            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.Update(deltaTime);
        }

        public void OverrideProgress(TimeSpan timeOverride)
        {
            General.Override(timeOverride);
            Transform.Override(timeOverride);
            LayerBrush?.BaseProperties?.Override(timeOverride);
            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.BaseProperties?.Override(timeOverride);
        }

        /// <inheritdoc />
        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint)
        {
            // Ensure the layer is ready
            if (Path == null || LayerShape?.Path == null || !General.PropertiesInitialized || !Transform.PropertiesInitialized)
                return;
            // Ensure the brush is ready
            if (LayerBrush?.BaseProperties?.PropertiesInitialized == false || LayerBrush?.BrushType != LayerBrushType.Regular)
                return;

            canvas.Save();
            canvas.ClipPath(Path);
            
            paint.BlendMode = General.BlendMode.CurrentValue;
            paint.Color = new SKColor(0, 0, 0, (byte) (Transform.Opacity.CurrentValue * 2.55f));

            // Pre-processing only affects other pre-processors and the brushes
            canvas.Save();
            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.InternalPreProcess(canvas, canvasInfo, new SKPath(Path), paint);

            // Shape clip must be determined before commiting to any rendering
            var shapeClip = CreateShapeClip();
            if (!shapeClip.IsEmpty)
                ExcludePathFromTranslation(shapeClip);

            if (!LayerBrush.SupportsTransformation)
                SimpleRender(canvas, canvasInfo, paint, shapeClip);
            else if (General.FillType.CurrentValue == LayerFillType.Stretch)
                StretchRender(canvas, canvasInfo, paint, shapeClip);
            else if (General.FillType.CurrentValue == LayerFillType.Clip)
                ClipRender(canvas, canvasInfo, paint, shapeClip);

            // Restore the canvas as to not be affected by pre-processors
            canvas.Restore();
            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.InternalPostProcess(canvas, canvasInfo, new SKPath(Path), paint);
            
            canvas.Restore();
        }

        private void SimpleRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath shapeClip)
        {
            var path = LayerShape.Path.Op(shapeClip, SKPathOp.Difference);
            LayerBrush.InternalRender(canvas, canvasInfo, path, paint);
        }

        private void StretchRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath shapeClip)
        {
            // Apply transformations
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            // Apply these before translation because anchorPosition takes translation into account
            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
            canvas.Scale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y);
            canvas.Translate(x, y);

            var path = LayerShape.Path.Op(shapeClip, SKPathOp.Difference);
            LayerBrush.InternalRender(canvas, canvasInfo, path, paint);
        }

        private void ClipRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath shapeClip)
        {
            // Apply transformation
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            var clipPath = new SKPath(LayerShape.Path);
            clipPath.Transform(SKMatrix.MakeTranslation(x, y));
            clipPath.Transform(SKMatrix.MakeScale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y));
            clipPath.Transform(SKMatrix.MakeRotationDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y));
            canvas.ClipPath(clipPath);

            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
            canvas.Translate(x, y);

            // Render the layer in the largest required bounds, this still creates stretching in some situations
            // but the only alternative I see right now is always forcing brushes to render on the entire canvas
            var boundsRect = new SKRect(
                Math.Min(clipPath.Bounds.Left - x, Bounds.Left - x),
                Math.Min(clipPath.Bounds.Top - y, Bounds.Top - y),
                Math.Max(clipPath.Bounds.Right - x, Bounds.Right - x),
                Math.Max(clipPath.Bounds.Bottom - y, Bounds.Bottom - y)
            );
            var renderPath = new SKPath();
            renderPath.AddRect(boundsRect);

            renderPath = renderPath.Op(shapeClip, SKPathOp.Difference);
            LayerBrush.InternalRender(canvas, canvasInfo, renderPath, paint);
        }

        internal void CalculateRenderProperties()
        {
            if (!Leds.Any())
                Path = new SKPath();
            else
            {
                var path = new SKPath {FillType = SKPathFillType.Winding};
                foreach (var artemisLed in Leds)
                    path.AddRect(artemisLed.AbsoluteRenderRectangle);

                Path = path;
            }

            // This is called here so that the shape's render properties are up to date when other code
            // responds to OnRenderPropertiesUpdated
            LayerShape?.CalculateRenderProperties();

            // Folder render properties are based on child paths and thus require an update
            if (Parent is Folder folder)
                folder.CalculateRenderProperties();

            OnRenderPropertiesUpdated();
        }

        internal SKPoint GetLayerAnchorPosition()
        {
            var positionProperty = Transform.Position.CurrentValue;

            // Start at the center of the shape
            var position = new SKPoint(Bounds.MidX, Bounds.MidY);

            // Apply translation
            position.X += positionProperty.X * Bounds.Width;
            position.Y += positionProperty.Y * Bounds.Height;

            return position;
        }


        /// <summary>
        ///     Excludes the provided path from the translations applied to the layer by applying translations that cancel the
        ///     layer translations out
        /// </summary>
        /// <param name="path"></param>
        public void ExcludePathFromTranslation(SKPath path)
        {
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition();
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - Bounds.MidX - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - Bounds.MidY - anchorProperty.Y * Bounds.Height;

            var reversedXScale = 1f / (sizeProperty.Width / 100f);
            var reversedYScale = 1f / (sizeProperty.Height / 100f);

            if (General.FillType == LayerFillType.Stretch)
            {
                path.Transform(SKMatrix.MakeRotationDegrees(rotationProperty * -1, anchorPosition.X, anchorPosition.Y));
                path.Transform(SKMatrix.MakeScale(reversedXScale, reversedYScale, anchorPosition.X, anchorPosition.Y));
                path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));
            }
            else
            {
                path.Transform(SKMatrix.MakeRotationDegrees(rotationProperty * -1, anchorPosition.X, anchorPosition.Y));
                path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));
            }
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
                var match = availableLeds.FirstOrDefault(a => a.Device.RgbDevice.GetDeviceIdentifier() == ledEntity.DeviceIdentifier &&
                                                              a.RgbLed.Id.ToString() == ledEntity.LedName);
                if (match != null)
                    leds.Add(match);
            }

            _leds = leds;
            CalculateRenderProperties();
        }

        #endregion

        #region Activation

        internal void Deactivate()
        {
            DeactivateLayerBrush();
            var layerEffects = new List<BaseLayerEffect>(LayerEffects);
            foreach (var baseLayerEffect in layerEffects)
                DeactivateLayerEffect(baseLayerEffect);
        }

        private void DeactivateLayerBrush()
        {
            if (LayerBrush == null)
                return;

            var brush = LayerBrush;
            LayerBrush = null;
            brush.Dispose();
        }

        internal void RemoveLayerBrush()
        {
            if (LayerBrush == null)
                return;

            var brush = LayerBrush;
            DeactivateLayerBrush();
            LayerEntity.PropertyEntities.RemoveAll(p => p.PluginGuid == brush.PluginInfo.Guid && p.Path.StartsWith("LayerBrush."));
        }

        #endregion

        #region Events

        public event EventHandler RenderPropertiesUpdated;
        public event EventHandler LayerBrushUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        internal void OnLayerBrushUpdated()
        {
            LayerBrushUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public enum LayerShapeType
    {
        Ellipse,
        Rectangle
    }

    public enum LayerFillType
    {
        Stretch,
        Clip
    }
}