using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    /// <summary>
    ///     Represents a layer on a profile. To create new layers use the <see cref="RenderElementService" /> by injecting
    ///     <see cref="IRenderElementService" /> into your code
    /// </summary>
    public sealed class Layer : RenderProfileElement
    {
        private LayerGeneralProperties _general;
        private SKBitmap _layerBitmap;
        private BaseLayerBrush _layerBrush;
        private LayerShape _layerShape;
        private List<ArtemisLed> _leds;
        private LayerTransformProperties _transform;

        internal Layer(Profile profile, ProfileElement parent, string name)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            Enabled = true;
            DisplayContinuously = true;
            General = new LayerGeneralProperties {IsCorePropertyGroup = true};
            Transform = new LayerTransformProperties {IsCorePropertyGroup = true};

            _layerEffects = new List<BaseLayerEffect>();
            _leds = new List<ArtemisLed>();
            _expandedPropertyGroups = new List<string>();

            General.PropertyGroupInitialized += GeneralOnPropertyGroupInitialized;
            ApplyRenderElementDefaults();
        }

        internal Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = layerEntity.Name;
            Enabled = layerEntity.Enabled;
            Order = layerEntity.Order;
            General = new LayerGeneralProperties {IsCorePropertyGroup = true};
            Transform = new LayerTransformProperties {IsCorePropertyGroup = true};

            _layerEffects = new List<BaseLayerEffect>();
            _leds = new List<ArtemisLed>();
            _expandedPropertyGroups = new List<string>();
            _expandedPropertyGroups.AddRange(layerEntity.ExpandedPropertyGroups);

            General.PropertyGroupInitialized += GeneralOnPropertyGroupInitialized;
            ApplyRenderElementEntity();
        }

        internal LayerEntity LayerEntity { get; set; }
        internal override RenderElementEntity RenderElementEntity => LayerEntity;

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
                SetAndNotify(ref _layerShape, value);
                if (Path != null)
                    CalculateRenderProperties();
            }
        }

        [PropertyGroupDescription(Name = "General", Description = "A collection of general properties")]
        public LayerGeneralProperties General
        {
            get => _general;
            set => SetAndNotify(ref _general, value);
        }

        [PropertyGroupDescription(Name = "Transform", Description = "A collection of transformation properties")]
        public LayerTransformProperties Transform
        {
            get => _transform;
            set => SetAndNotify(ref _transform, value);
        }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape" />.
        /// </summary>
        public BaseLayerBrush LayerBrush
        {
            get => _layerBrush;
            internal set => SetAndNotify(ref _layerBrush, value);
        }

        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        /// <inheritdoc />
        public override List<BaseLayerPropertyKeyframe> GetAllKeyframes()
        {
            var keyframes = base.GetAllKeyframes();

            foreach (var baseLayerProperty in General.GetAllLayerProperties())
                keyframes.AddRange(baseLayerProperty.BaseKeyframes);
            foreach (var baseLayerProperty in Transform.GetAllLayerProperties())
                keyframes.AddRange(baseLayerProperty.BaseKeyframes);
            if (LayerBrush?.BaseProperties != null)
            {
                foreach (var baseLayerProperty in LayerBrush.BaseProperties.GetAllLayerProperties())
                    keyframes.AddRange(baseLayerProperty.BaseKeyframes);
            }

            return keyframes;
        }

        #region Storage

        internal override void ApplyToEntity()
        {
            // Properties
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();
            LayerEntity.Order = Order;
            LayerEntity.Enabled = Enabled;
            LayerEntity.Name = Name;
            LayerEntity.ProfileId = Profile.EntityId;
            LayerEntity.ExpandedPropertyGroups.Clear();
            LayerEntity.ExpandedPropertyGroups.AddRange(_expandedPropertyGroups);

            General.ApplyToEntity();
            Transform.ApplyToEntity();
            LayerBrush?.BaseProperties.ApplyToEntity();

            // Effects
            ApplyRenderElementToEntity();

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

            // Conditions
            RenderElementEntity.RootDisplayCondition = DisplayConditionGroup?.DisplayConditionGroupEntity;
            DisplayConditionGroup?.ApplyToEntity();
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
            if (!Enabled || LayerBrush?.BaseProperties == null || !LayerBrush.BaseProperties.PropertiesInitialized)
                return;

            // Ensure the layer must still be displayed
            UpdateDisplayCondition();

            // Update the layer timeline, this will give us a new delta time which could be negative in case the main segment wrapped back
            // to it's start
            UpdateTimeline(deltaTime);

            // No point updating further than this if the layer is not going to be rendered
            if (TimelinePosition > TimelineLength)
                return;

            General.Update();
            Transform.Update();
            LayerBrush.BaseProperties?.Update();
            LayerBrush.Update(deltaTime);

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update();
                baseLayerEffect.Update(deltaTime);
            }
        }

        public override void OverrideProgress(TimeSpan timeOverride, bool stickToMainSegment)
        {
            if (!Enabled || LayerBrush?.BaseProperties == null || !LayerBrush.BaseProperties.PropertiesInitialized)
                return;

            var beginTime = TimelinePosition;

            if (stickToMainSegment)
            {
                if (!DisplayContinuously)
                {
                    var position = timeOverride + StartSegmentLength;
                    if (position > StartSegmentLength + EndSegmentLength)
                        TimelinePosition = StartSegmentLength + EndSegmentLength;
                }
                else
                {
                    var progress = timeOverride.TotalMilliseconds % MainSegmentLength.TotalMilliseconds;
                    if (progress > 0)
                        TimelinePosition = TimeSpan.FromMilliseconds(progress) + StartSegmentLength;
                    else
                        TimelinePosition = StartSegmentLength;
                }
            }
            else
                TimelinePosition = timeOverride;

            var delta = (TimelinePosition - beginTime).TotalSeconds;

            General.Update();
            Transform.Update();
            LayerBrush.BaseProperties?.Update();
            LayerBrush.Update(delta);

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update();
                baseLayerEffect.Update(delta);
            }
        }

        /// <inheritdoc />
        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            if (!Enabled || TimelinePosition > TimelineLength)
                return;

            // Ensure the layer is ready
            if (Path == null || LayerShape?.Path == null || !General.PropertiesInitialized || !Transform.PropertiesInitialized)
                return;
            // Ensure the brush is ready
            if (LayerBrush?.BaseProperties?.PropertiesInitialized == false || LayerBrush?.BrushType != LayerBrushType.Regular)
                return;

            if (_layerBitmap == null)
                _layerBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            else if (_layerBitmap.Info.Width != (int) Path.Bounds.Width || _layerBitmap.Info.Height != (int) Path.Bounds.Height)
            {
                _layerBitmap.Dispose();
                _layerBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            }

            using var layerPath = new SKPath(Path);
            using var layerCanvas = new SKCanvas(_layerBitmap);
            using var layerPaint = new SKPaint
            {
                FilterQuality = SKFilterQuality.Low,
                BlendMode = General.BlendMode.CurrentValue,
                Color = new SKColor(0, 0, 0, (byte) (Transform.Opacity.CurrentValue * 2.55f))
            };
            layerCanvas.Clear();

            layerPath.Transform(SKMatrix.MakeTranslation(layerPath.Bounds.Left * -1, layerPath.Bounds.Top * -1));

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PreProcess(layerCanvas, _layerBitmap.Info, layerPath, layerPaint);

            // No point rendering if the alpha was set to zero by one of the effects
            if (layerPaint.Color.Alpha == 0)
                return;

            layerCanvas.ClipPath(layerPath);

            if (!LayerBrush.SupportsTransformation)
                SimpleRender(layerCanvas, _layerBitmap.Info, layerPaint, layerPath);
            else if (General.FillType.CurrentValue == LayerFillType.Stretch)
                StretchRender(layerCanvas, _layerBitmap.Info, layerPaint, layerPath);
            else if (General.FillType.CurrentValue == LayerFillType.Clip)
                ClipRender(layerCanvas, _layerBitmap.Info, layerPaint, layerPath);

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PostProcess(layerCanvas, _layerBitmap.Info, layerPath, layerPaint);

            var targetLocation = new SKPoint(0, 0);
            if (Parent is Folder parentFolder)
                targetLocation = Path.Bounds.Location - parentFolder.Path.Bounds.Location;

            
            canvas.DrawBitmap(_layerBitmap, targetLocation, layerPaint);
        }

        private void SimpleRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath layerPath)
        {
            using var renderPath = new SKPath(LayerShape.Path);
            LayerBrush.InternalRender(canvas, canvasInfo, renderPath, paint);
        }

        private void StretchRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath layerPath)
        {
            // Apply transformations
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition(layerPath);
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - layerPath.Bounds.MidX - anchorProperty.X * layerPath.Bounds.Width;
            var y = anchorPosition.Y - layerPath.Bounds.MidY - anchorProperty.Y * layerPath.Bounds.Height;

            // Apply these before translation because anchorPosition takes translation into account
            canvas.RotateDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
            canvas.Scale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y);
            canvas.Translate(x, y);

            using var renderPath = new SKPath(LayerShape.Path);
            LayerBrush.InternalRender(canvas, canvasInfo, renderPath, paint);
        }

        private void ClipRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint, SKPath layerPath)
        {
            // Apply transformation
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition(layerPath);
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - layerPath.Bounds.MidX - anchorProperty.X * layerPath.Bounds.Width;
            var y = anchorPosition.Y - layerPath.Bounds.MidY - anchorProperty.Y * layerPath.Bounds.Height;

            using var clipPath = new SKPath(LayerShape.Path);
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
            using var renderPath = new SKPath();
            renderPath.AddRect(boundsRect);

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

        internal SKPoint GetLayerAnchorPosition(SKPath layerPath, bool zeroBased = false)
        {
            var positionProperty = Transform.Position.CurrentValue;

            // Start at the center of the shape
            var position = zeroBased
                ? new SKPoint(layerPath.Bounds.MidX - layerPath.Bounds.Left, layerPath.Bounds.MidY - layerPath.Bounds.Top)
                : new SKPoint(layerPath.Bounds.MidX, layerPath.Bounds.MidY);

            // Apply translation
            position.X += positionProperty.X * layerPath.Bounds.Width;
            position.Y += positionProperty.Y * layerPath.Bounds.Height;

            return position;
        }

        /// <summary>
        ///     Excludes the provided path from the translations applied to the layer by applying translations that cancel the
        ///     layer translations out
        /// </summary>
        /// <param name="path"></param>
        public void IncludePathInTranslation(SKPath path, bool zeroBased)
        {
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition(Path, zeroBased);
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - (zeroBased ? Bounds.MidX - Bounds.Left : Bounds.MidX) - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - (zeroBased ? Bounds.MidY - Bounds.Top : Bounds.MidY) - anchorProperty.Y * Bounds.Height;

            if (General.FillType == LayerFillType.Stretch)
            {
                path.Transform(SKMatrix.MakeTranslation(x, y));
                path.Transform(SKMatrix.MakeScale((sizeProperty.Width / 100f), (sizeProperty.Height / 100f), anchorPosition.X, anchorPosition.Y));
                path.Transform(SKMatrix.MakeRotationDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y));
            }
            else
            {
                path.Transform(SKMatrix.MakeTranslation(x, y));
                path.Transform(SKMatrix.MakeRotationDegrees(rotationProperty * -1, anchorPosition.X, anchorPosition.Y));
            }
        }

        /// <summary>
        ///     Excludes the provided path from the translations applied to the layer by applying translations that cancel the
        ///     layer translations out
        /// </summary>
        public void ExcludePathFromTranslation(SKPath path, bool zeroBased)
        {
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition(Path, zeroBased);
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - (zeroBased ? Bounds.MidX - Bounds.Left : Bounds.MidX) - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - (zeroBased ? Bounds.MidY - Bounds.Top : Bounds.MidY) - anchorProperty.Y * Bounds.Height;

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

        /// <summary>
        ///     Excludes the provided canvas from the translations applied to the layer by applying translations that cancel the
        ///     layer translations out
        /// </summary>
        /// <returns>The number of transformations applied</returns>
        public int ExcludeCanvasFromTranslation(SKCanvas canvas, bool zeroBased)
        {
            var sizeProperty = Transform.Scale.CurrentValue;
            var rotationProperty = Transform.Rotation.CurrentValue;

            var anchorPosition = GetLayerAnchorPosition(Path, zeroBased);
            var anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            var x = anchorPosition.X - (zeroBased ? Bounds.MidX - Bounds.Left : Bounds.MidX) - anchorProperty.X * Bounds.Width;
            var y = anchorPosition.Y - (zeroBased ? Bounds.MidY - Bounds.Top : Bounds.MidY) - anchorProperty.Y * Bounds.Height;

            var reversedXScale = 1f / (sizeProperty.Width / 100f);
            var reversedYScale = 1f / (sizeProperty.Height / 100f);

            if (General.FillType == LayerFillType.Stretch)
            {
                canvas.Translate(x * -1, y * -1);
                canvas.Scale(reversedXScale, reversedYScale, anchorPosition.X, anchorPosition.Y);
                canvas.RotateDegrees(rotationProperty * -1, anchorPosition.X, anchorPosition.Y);

                return 3;
            }

            canvas.RotateDegrees(rotationProperty * -1, anchorPosition.X, anchorPosition.Y);
            canvas.Translate(x * -1, y * -1);
            return 2;
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
            _layerBitmap?.Dispose();
            _layerBitmap = null;

            DeactivateLayerBrush();
            var layerEffects = new List<BaseLayerEffect>(LayerEffects);
            foreach (var baseLayerEffect in layerEffects)
                DeactivateLayerEffect(baseLayerEffect);
        }

        internal void DeactivateLayerBrush()
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