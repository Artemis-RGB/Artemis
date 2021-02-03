using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer in a <see cref="Profile" />
    /// </summary>
    public sealed class Layer : RenderProfileElement
    {
        private LayerGeneralProperties _general;
        private BaseLayerBrush? _layerBrush;
        private LayerShape? _layerShape;
        private List<ArtemisLed> _leds;
        private LayerTransformProperties _transform;

        /// <summary>
        ///     Creates a new instance of the <see cref="Layer" /> class and adds itself to the child collection of the provided
        ///     <paramref name="parent" />
        /// </summary>
        /// <param name="parent">The parent of the layer</param>
        /// <param name="name">The name of the layer</param>
        public Layer(ProfileElement parent, string name) : base(parent.Profile)
        {
            LayerEntity = new LayerEntity();
            EntityId = Guid.NewGuid();

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Profile = Parent.Profile;
            Name = name;
            Enabled = true;
            _general = new LayerGeneralProperties();
            _transform = new LayerTransformProperties();

            _leds = new List<ArtemisLed>();

            Initialize();
            Parent.AddChild(this, 0);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Layer" /> class based on the provided layer entity
        /// </summary>
        /// <param name="profile">The profile the layer belongs to</param>
        /// <param name="parent">The parent of the layer</param>
        /// <param name="layerEntity">The entity of the layer</param>
        public Layer(Profile profile, ProfileElement parent, LayerEntity layerEntity) : base(parent.Profile)
        {
            LayerEntity = layerEntity;
            EntityId = layerEntity.Id;

            Profile = profile;
            Parent = parent;
            _general = new LayerGeneralProperties();
            _transform = new LayerTransformProperties();

            _leds = new List<ArtemisLed>();

            Load();
            Initialize();
        }

        /// <summary>
        ///     A collection of all the LEDs this layer is assigned to.
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds => _leds.AsReadOnly();

        /// <summary>
        ///     Defines the shape that is rendered by the <see cref="LayerBrush" />.
        /// </summary>
        public LayerShape? LayerShape
        {
            get => _layerShape;
            set
            {
                SetAndNotify(ref _layerShape, value);
                if (Path != null)
                    CalculateRenderProperties();
            }
        }

        /// <summary>
        ///     Gets the general properties of the layer
        /// </summary>
        [PropertyGroupDescription(Name = "General", Description = "A collection of general properties")]
        public LayerGeneralProperties General
        {
            get => _general;
            private set => SetAndNotify(ref _general, value);
        }

        /// <summary>
        ///     Gets the transform properties of the layer
        /// </summary>
        [PropertyGroupDescription(Name = "Transform", Description = "A collection of transformation properties")]
        public LayerTransformProperties Transform
        {
            get => _transform;
            private set => SetAndNotify(ref _transform, value);
        }

        /// <summary>
        ///     The brush that will fill the <see cref="LayerShape" />.
        /// </summary>
        public BaseLayerBrush? LayerBrush
        {
            get => _layerBrush;
            internal set => SetAndNotify(ref _layerBrush, value);
        }

        /// <summary>
        ///     Gets the layer entity this layer uses for persistent storage
        /// </summary>
        public LayerEntity LayerEntity { get; internal set; }

        internal override RenderElementEntity RenderElementEntity => LayerEntity;

        /// <inheritdoc />
        public override List<ILayerProperty> GetAllLayerProperties()
        {
            List<ILayerProperty> result = new();
            result.AddRange(General.GetAllLayerProperties());
            result.AddRange(Transform.GetAllLayerProperties());
            if (LayerBrush?.BaseProperties != null)
                result.AddRange(LayerBrush.BaseProperties.GetAllLayerProperties());
            foreach (BaseLayerEffect layerEffect in LayerEffects)
                if (layerEffect.BaseProperties != null)
                    result.AddRange(layerEffect.BaseProperties.GetAllLayerProperties());

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Layer] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            Disposed = true;

            // Brush first in case it depends on any of the other disposables during it's own disposal
            _layerBrush?.Dispose();
            _general.Dispose();
            _transform.Dispose();
            Renderer.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        private void Initialize()
        {
            LayerBrushStore.LayerBrushAdded += LayerBrushStoreOnLayerBrushAdded;
            LayerBrushStore.LayerBrushRemoved += LayerBrushStoreOnLayerBrushRemoved;

            // Layers have two hardcoded property groups, instantiate them
            Attribute generalAttribute = Attribute.GetCustomAttribute(
                GetType().GetProperty(nameof(General))!,
                typeof(PropertyGroupDescriptionAttribute)
            )!;
            Attribute transformAttribute = Attribute.GetCustomAttribute(
                GetType().GetProperty(nameof(Transform))!,
                typeof(PropertyGroupDescriptionAttribute)
            )!;
            General.GroupDescription = (PropertyGroupDescriptionAttribute) generalAttribute;
            General.Initialize(this, "General.", Constants.CorePluginFeature);
            Transform.GroupDescription = (PropertyGroupDescriptionAttribute) transformAttribute;
            Transform.Initialize(this, "Transform.", Constants.CorePluginFeature);

            General.ShapeType.CurrentValueSet += ShapeTypeOnCurrentValueSet;
            ApplyShapeType();
            ActivateLayerBrush();
            
            Reset();
        }

        #region Storage

        internal override void Load()
        {
            EntityId = LayerEntity.Id;
            Name = LayerEntity.Name;
            Enabled = LayerEntity.Enabled;
            Order = LayerEntity.Order;

            ExpandedPropertyGroups.AddRange(LayerEntity.ExpandedPropertyGroups);
            LoadRenderElement();
        }

        internal override void Save()
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            // Properties
            LayerEntity.Id = EntityId;
            LayerEntity.ParentId = Parent?.EntityId ?? new Guid();
            LayerEntity.Order = Order;
            LayerEntity.Enabled = Enabled;
            LayerEntity.Name = Name;
            LayerEntity.ProfileId = Profile.EntityId;
            LayerEntity.ExpandedPropertyGroups.Clear();
            LayerEntity.ExpandedPropertyGroups.AddRange(ExpandedPropertyGroups);

            General.ApplyToEntity();
            Transform.ApplyToEntity();
            LayerBrush?.BaseProperties?.ApplyToEntity();

            // LEDs
            LayerEntity.Leds.Clear();
            foreach (ArtemisLed artemisLed in Leds)
            {
                LedEntity ledEntity = new()
                {
                    DeviceIdentifier = artemisLed.Device.RgbDevice.GetDeviceIdentifier(),
                    LedName = artemisLed.RgbLed.Id.ToString()
                };
                LayerEntity.Leds.Add(ledEntity);
            }

            SaveRenderElement();
        }

        #endregion

        #region Shape management

        private void ShapeTypeOnCurrentValueSet(object? sender, EventArgs e)
        {
            ApplyShapeType();
        }

        private void ApplyShapeType()
        {
            LayerShape = General.ShapeType.CurrentValue switch
            {
                LayerShapeType.Ellipse => new EllipseShape(this),
                LayerShapeType.Rectangle => new RectangleShape(this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        #region Rendering

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            if (!Enabled)
                return;

            UpdateDisplayCondition();
            UpdateTimeline(deltaTime);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            DisplayConditionMet = false;
            Timeline.JumpToEnd();
        }

        /// <inheritdoc />
        public override void Render(SKCanvas canvas)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            // Ensure the layer is ready
            if (!Enabled || Path == null || LayerShape?.Path == null || !General.PropertiesInitialized || !Transform.PropertiesInitialized)
                return;
            // Ensure the brush is ready
            if (LayerBrush?.BaseProperties?.PropertiesInitialized == false || LayerBrush?.BrushType != LayerBrushType.Regular)
                return;

            RenderTimeline(Timeline, canvas);
            foreach (Timeline extraTimeline in Timeline.ExtraTimelines.ToList())
                RenderTimeline(extraTimeline, canvas);
            Timeline.ClearDelta();
        }

        private void ApplyTimeline(Timeline timeline)
        {
            General.Update(timeline);
            Transform.Update(timeline);
            if (LayerBrush != null)
            {
                LayerBrush.BaseProperties?.Update(timeline);
                LayerBrush.Update(timeline.Delta.TotalSeconds);
            }

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update(timeline);
                baseLayerEffect.Update(timeline.Delta.TotalSeconds);
            }
        }

        private void RenderTimeline(Timeline timeline, SKCanvas canvas)
        {
            if (Path == null || LayerBrush == null)
                throw new ArtemisCoreException("The layer is not yet ready for rendering");

            if (timeline.IsFinished)
                return;

            ApplyTimeline(timeline);

            try
            {
                canvas.Save();
                Renderer.Open(Path, Parent as Folder);

                if (Renderer.Canvas == null || Renderer.Path == null || Renderer.Paint == null)
                    throw new ArtemisCoreException("Failed to open layer render context");

                // Apply blend mode and color
                Renderer.Paint.BlendMode = General.BlendMode.CurrentValue;
                Renderer.Paint.Color = new SKColor(0, 0, 0, (byte) (Transform.Opacity.CurrentValue * 2.55f));

                using SKPath renderPath = new();
                if (General.ShapeType.CurrentValue == LayerShapeType.Rectangle)
                    renderPath.AddRect(Renderer.Path.Bounds);
                else
                    renderPath.AddOval(Renderer.Path.Bounds);

                if (General.TransformMode.CurrentValue == LayerTransformMode.Normal)
                {
                    // Apply transformation except rotation to the render path
                    if (LayerBrush.SupportsTransformation)
                    {
                        SKMatrix renderPathMatrix = GetTransformMatrix(true, true, true, false);
                        renderPath.Transform(renderPathMatrix);
                    }

                    // Apply rotation to the canvas
                    if (LayerBrush.SupportsTransformation)
                    {
                        SKMatrix rotationMatrix = GetTransformMatrix(true, false, false, true);
                        Renderer.Canvas.SetMatrix(Renderer.Canvas.TotalMatrix.PreConcat(rotationMatrix));
                    }

                    // If a brush is a bad boy and tries to color outside the lines, ensure that its clipped off
                    Renderer.Canvas.ClipPath(renderPath);
                    DelegateRendering(renderPath.Bounds);
                }
                else if (General.TransformMode.CurrentValue == LayerTransformMode.Clip)
                {
                    SKMatrix renderPathMatrix = GetTransformMatrix(true, true, true, true);
                    renderPath.Transform(renderPathMatrix);

                    // If a brush is a bad boy and tries to color outside the lines, ensure that its clipped off
                    Renderer.Canvas.ClipPath(renderPath);
                    DelegateRendering(Renderer.Path.Bounds);
                }

                canvas.DrawBitmap(Renderer.Bitmap, Renderer.TargetLocation, Renderer.Paint);
            }
            finally
            {
                try
                {
                    canvas.Restore();
                }
                catch
                {
                    // ignored
                }

                Renderer.Close();
            }
        }

        private void DelegateRendering(SKRect bounds)
        {
            if (LayerBrush == null)
                throw new ArtemisCoreException("The layer is not yet ready for rendering");
            if (Renderer.Canvas == null || Renderer.Paint == null)
                throw new ArtemisCoreException("Failed to open layer render context");

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PreProcess(Renderer.Canvas, bounds, Renderer.Paint);

            LayerBrush.InternalRender(Renderer.Canvas, bounds, Renderer.Paint);

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PostProcess(Renderer.Canvas, bounds, Renderer.Paint);
        }

        internal void CalculateRenderProperties()
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            if (!Leds.Any())
            {
                Path = new SKPath();
            }
            else
            {
                SKPath path = new() {FillType = SKPathFillType.Winding};
                foreach (ArtemisLed artemisLed in Leds)
                    path.AddRect(artemisLed.AbsoluteRectangle);

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

        internal SKPoint GetLayerAnchorPosition(SKPath layerPath, bool applyTranslation, bool zeroBased)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            SKPoint positionProperty = Transform.Position.CurrentValue;

            // Start at the center of the shape
            SKPoint position = zeroBased
                ? new SKPoint(layerPath.Bounds.MidX - layerPath.Bounds.Left, layerPath.Bounds.MidY - layerPath.Bounds.Top)
                : new SKPoint(layerPath.Bounds.MidX, layerPath.Bounds.MidY);

            // Apply translation
            if (applyTranslation)
            {
                position.X += positionProperty.X * layerPath.Bounds.Width;
                position.Y += positionProperty.Y * layerPath.Bounds.Height;
            }

            return position;
        }

        /// <summary>
        ///     Creates a transformation matrix that applies the current transformation settings
        /// </summary>
        /// <param name="zeroBased">
        ///     If true, treats the layer as if it is located at 0,0 instead of its actual position on the
        ///     surface
        /// </param>
        /// <param name="includeTranslation">Whether translation should be included</param>
        /// <param name="includeScale">Whether the scale should be included</param>
        /// <param name="includeRotation">Whether the rotation should be included</param>
        /// <returns>The transformation matrix containing the current transformation settings</returns>
        public SKMatrix GetTransformMatrix(bool zeroBased, bool includeTranslation, bool includeScale, bool includeRotation)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            if (Path == null)
                return SKMatrix.Empty;

            SKSize sizeProperty = Transform.Scale.CurrentValue;
            float rotationProperty = Transform.Rotation.CurrentValue;

            SKPoint anchorPosition = GetLayerAnchorPosition(Path, true, zeroBased);
            SKPoint anchorProperty = Transform.AnchorPoint.CurrentValue;

            // Translation originates from the unscaled center of the shape and is tied to the anchor
            float x = anchorPosition.X - (zeroBased ? Bounds.MidX - Bounds.Left : Bounds.MidX) - anchorProperty.X * Bounds.Width;
            float y = anchorPosition.Y - (zeroBased ? Bounds.MidY - Bounds.Top : Bounds.MidY) - anchorProperty.Y * Bounds.Height;

            SKMatrix transform = SKMatrix.Empty;

            if (includeTranslation)
                // transform is always SKMatrix.Empty here...
                transform = SKMatrix.CreateTranslation(x, y);

            if (includeScale)
            {
                if (transform == SKMatrix.Empty)
                    transform = SKMatrix.CreateScale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y);
                else
                    transform = transform.PostConcat(SKMatrix.CreateScale(sizeProperty.Width / 100f, sizeProperty.Height / 100f, anchorPosition.X, anchorPosition.Y));
            }

            if (includeRotation)
            {
                if (transform == SKMatrix.Empty)
                    transform = SKMatrix.CreateRotationDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y);
                else
                    transform = transform.PostConcat(SKMatrix.CreateRotationDegrees(rotationProperty, anchorPosition.X, anchorPosition.Y));
            }

            return transform;
        }

        #endregion

        #region LED management

        /// <summary>
        ///     Adds a new <see cref="ArtemisLed" /> to the layer and updates the render properties.
        /// </summary>
        /// <param name="led">The LED to add</param>
        public void AddLed(ArtemisLed led)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            _leds.Add(led);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Adds a collection of new <see cref="ArtemisLed" />s to the layer and updates the render properties.
        /// </summary>
        /// <param name="leds">The LEDs to add</param>
        public void AddLeds(IEnumerable<ArtemisLed> leds)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            _leds.AddRange(leds);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Removes a <see cref="ArtemisLed" /> from the layer and updates the render properties.
        /// </summary>
        /// <param name="led">The LED to remove</param>
        public void RemoveLed(ArtemisLed led)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            _leds.Remove(led);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Removes all <see cref="ArtemisLed" />s from the layer and updates the render properties.
        /// </summary>
        public void ClearLeds()
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            _leds.Clear();
            CalculateRenderProperties();
        }

        internal void PopulateLeds(ArtemisSurface surface)
        {
            if (Disposed)
                throw new ObjectDisposedException("Layer");

            List<ArtemisLed> leds = new();

            // Get the surface LEDs for this layer
            List<ArtemisLed> availableLeds = surface.Devices.Where(d => d.IsEnabled).SelectMany(d => d.Leds).ToList();
            foreach (LedEntity ledEntity in LayerEntity.Leds)
            {
                ArtemisLed? match = availableLeds.FirstOrDefault(a => a.Device.RgbDevice.GetDeviceIdentifier() == ledEntity.DeviceIdentifier &&
                                                                      a.RgbLed.Id.ToString() == ledEntity.LedName);
                if (match != null)
                    leds.Add(match);
            }

            _leds = leds;
            CalculateRenderProperties();
        }

        #endregion

        #region Brush management

        /// <summary>
        ///     Changes the current layer brush to the brush described in the provided <paramref name="descriptor" />
        /// </summary>
        public void ChangeLayerBrush(LayerBrushDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (LayerBrush != null)
            {
                BaseLayerBrush brush = LayerBrush;
                LayerBrush = null;
                brush.Dispose();
            }

            // Ensure the brush reference matches the brush
            LayerBrushReference? current = General.BrushReference.BaseValue;
            if (!descriptor.MatchesLayerBrushReference(current))
                General.BrushReference.BaseValue = new LayerBrushReference(descriptor);

            ActivateLayerBrush();
        }

        /// <summary>
        ///     Removes the current layer brush from the layer
        /// </summary>
        public void RemoveLayerBrush()
        {
            if (LayerBrush == null)
                return;

            BaseLayerBrush brush = LayerBrush;
            DeactivateLayerBrush();
            LayerEntity.PropertyEntities.RemoveAll(p => p.FeatureId == brush.ProviderId && p.Path.StartsWith("LayerBrush."));
        }

        internal void ActivateLayerBrush()
        {
            LayerBrushReference? current = General.BrushReference.CurrentValue;
            if (current == null)
                return;

            LayerBrushDescriptor? descriptor = current.LayerBrushProviderId != null && current.BrushType != null
                ? LayerBrushStore.Get(current.LayerBrushProviderId, current.BrushType)?.LayerBrushDescriptor
                : null;
            descriptor?.CreateInstance(this);

            OnLayerBrushUpdated();
        }

        internal void DeactivateLayerBrush()
        {
            if (LayerBrush == null)
                return;

            BaseLayerBrush brush = LayerBrush;
            LayerBrush = null;
            brush.Dispose();

            OnLayerBrushUpdated();
        }

        #endregion

        #region Event handlers

        private void LayerBrushStoreOnLayerBrushRemoved(object? sender, LayerBrushStoreEvent e)
        {
            if (LayerBrush?.Descriptor == e.Registration.LayerBrushDescriptor)
                DeactivateLayerBrush();
        }

        private void LayerBrushStoreOnLayerBrushAdded(object? sender, LayerBrushStoreEvent e)
        {
            if (LayerBrush != null || !General.PropertiesInitialized)
                return;

            LayerBrushReference? current = General.BrushReference.CurrentValue;
            if (e.Registration.PluginFeature.Id == current?.LayerBrushProviderId &&
                e.Registration.LayerBrushDescriptor.LayerBrushType.Name == current.BrushType)
                ActivateLayerBrush();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a property affecting the rendering properties of this layer has been updated
        /// </summary>
        public event EventHandler? RenderPropertiesUpdated;

        /// <summary>
        ///     Occurs when the layer brush of this layer has been updated
        /// </summary>
        public event EventHandler? LayerBrushUpdated;

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

    /// <summary>
    ///     Represents a type of layer shape
    /// </summary>
    public enum LayerShapeType
    {
        /// <summary>
        ///     A circular layer shape
        /// </summary>
        Ellipse,

        /// <summary>
        ///     A rectangular layer shape
        /// </summary>
        Rectangle
    }

    /// <summary>
    ///     Represents a layer transform mode
    /// </summary>
    public enum LayerTransformMode
    {
        /// <summary>
        ///     Normal transformation
        /// </summary>
        Normal,

        /// <summary>
        ///     Transforms only a clip
        /// </summary>
        Clip
    }
}