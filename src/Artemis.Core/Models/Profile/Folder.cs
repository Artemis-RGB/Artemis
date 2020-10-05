using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a folder in a <see cref="Profile" />
    /// </summary>
    public sealed class Folder : RenderProfileElement
    {
        private SKBitmap _folderBitmap;

        /// <summary>
        ///     Creates a new instance of the <see cref="Folder" /> class and adds itself to the child collection of the provided
        ///     <paramref name="parent" />
        /// </summary>
        /// <param name="parent">The parent of the folder</param>
        /// <param name="name">The name of the folder</param>
        public Folder(ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Profile = Parent.Profile;
            Name = name;
            Enabled = true;
            DisplayContinuously = true;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();

            ApplyRenderElementDefaults();
            Parent.AddChild(this);
        }

        internal Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity)
        {
            FolderEntity = folderEntity;

            EntityId = folderEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = folderEntity.Name;
            Enabled = folderEntity.Enabled;
            Order = folderEntity.Order;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();
            Load();
        }

        internal FolderEntity FolderEntity { get; set; }

        /// <inheritdoc />
        public override List<ILayerProperty> GetAllLayerProperties()
        {
            List<ILayerProperty> result = new List<ILayerProperty>();
            foreach (BaseLayerEffect layerEffect in LayerEffects)
            {
                if (layerEffect.BaseProperties != null)
                    result.AddRange(layerEffect.BaseProperties.GetAllLayerProperties());
            }

            return result;
        }

        internal override RenderElementEntity RenderElementEntity => FolderEntity;
        public bool IsRootFolder => Parent == Profile;

        public override void Update(double deltaTime)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled)
                return;

            // Disable data bindings during an override
            bool wasApplyingDataBindings = ApplyDataBindingsEnabled;
            ApplyDataBindingsEnabled = false;

            UpdateDisplayCondition();

            // Update the layer timeline, this will give us a new delta time which could be negative in case the main segment wrapped back
            // to it's start
            UpdateTimeline(deltaTime);

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update(deltaTime);
                baseLayerEffect.Update(deltaTime);
            }

            // Iterate the children in reverse because that's how they must be rendered too
            for (int index = Children.Count - 1; index > -1; index--)
            {
                ProfileElement profileElement = Children[index];
                profileElement.Update(deltaTime);
            }

            // Restore the old data bindings enabled state
            ApplyDataBindingsEnabled = wasApplyingDataBindings;
        }

        protected internal override void UpdateTimelineLength()
        {
            TimelineLength = !Children.Any() ? TimeSpan.Zero : Children.OfType<RenderProfileElement>().Max(c => c.TimelineLength);
            if (StartSegmentLength + MainSegmentLength + EndSegmentLength > TimelineLength)
                TimelineLength = StartSegmentLength + MainSegmentLength + EndSegmentLength;

            if (Parent is RenderProfileElement parent)
                parent.UpdateTimelineLength();
        }

        public override void OverrideProgress(TimeSpan timeOverride, bool stickToMainSegment)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled)
                return;

            TimeSpan beginTime = TimelinePosition;

            if (stickToMainSegment)
            {
                if (!DisplayContinuously)
                {
                    TimeSpan position = timeOverride + StartSegmentLength;
                    if (position > StartSegmentLength + EndSegmentLength)
                        TimelinePosition = StartSegmentLength + EndSegmentLength;
                }
                else
                {
                    double progress = timeOverride.TotalMilliseconds % MainSegmentLength.TotalMilliseconds;
                    if (progress > 0)
                        TimelinePosition = TimeSpan.FromMilliseconds(progress) + StartSegmentLength;
                    else
                        TimelinePosition = StartSegmentLength;
                }
            }
            else
                TimelinePosition = timeOverride;

            double delta = (TimelinePosition - beginTime).TotalSeconds;

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update(delta);
                baseLayerEffect.Update(delta);
            }
        }

        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            if (Path == null || !Enabled || !Children.Any(c => c.Enabled))
                return;

            // No need to render if at the end of the timeline
            if (TimelinePosition > TimelineLength)
                return;

            if (_folderBitmap == null)
                _folderBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            else if (_folderBitmap.Info.Width != (int) Path.Bounds.Width || _folderBitmap.Info.Height != (int) Path.Bounds.Height)
            {
                _folderBitmap.Dispose();
                _folderBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            }

            using SKPath folderPath = new SKPath(Path);
            using SKCanvas folderCanvas = new SKCanvas(_folderBitmap);
            using SKPaint folderPaint = new SKPaint();
            folderCanvas.Clear();

            folderPath.Transform(SKMatrix.MakeTranslation(folderPath.Bounds.Left * -1, folderPath.Bounds.Top * -1));

            SKPoint targetLocation = Path.Bounds.Location;
            if (Parent is Folder parentFolder)
                targetLocation -= parentFolder.Path.Bounds.Location;

            canvas.Save();

            using SKPath clipPath = new SKPath(folderPath);
            clipPath.Transform(SKMatrix.MakeTranslation(targetLocation.X, targetLocation.Y));
            canvas.ClipPath(clipPath);

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PreProcess(folderCanvas, _folderBitmap.Info, folderPath, folderPaint);

            // No point rendering if the alpha was set to zero by one of the effects
            if (folderPaint.Color.Alpha == 0)
                return;

            // Iterate the children in reverse because the first layer must be rendered last to end up on top
            for (int index = Children.Count - 1; index > -1; index--)
            {
                folderCanvas.Save();
                ProfileElement profileElement = Children[index];
                profileElement.Render(deltaTime, folderCanvas, _folderBitmap.Info);
                folderCanvas.Restore();
            }

            // If required, apply the opacity override of the module to the root folder
            if (IsRootFolder && Profile.Module.OpacityOverride < 1)
            {
                double multiplier = Easings.SineEaseInOut(Profile.Module.OpacityOverride);
                folderPaint.Color = folderPaint.Color.WithAlpha((byte) (folderPaint.Color.Alpha * multiplier));
            }

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PostProcess(canvas, canvasInfo, folderPath, folderPaint);
            canvas.DrawBitmap(_folderBitmap, targetLocation, folderPaint);

            canvas.Restore();
        }

        /// <inheritdoc />
        public override void AddChild(ProfileElement child, int? order = null)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            base.AddChild(child, order);
            UpdateTimelineLength();
            CalculateRenderProperties();
        }

        /// <inheritdoc />
        public override void RemoveChild(ProfileElement child)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            base.RemoveChild(child);
            UpdateTimelineLength();
            CalculateRenderProperties();
        }

        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        public void CalculateRenderProperties()
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            SKPath path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (ProfileElement child in Children)
            {
                if (child is RenderProfileElement effectChild && effectChild.Path != null)
                    path.AddPath(effectChild.Path);
            }

            Path = path;

            // Folder render properties are based on child paths and thus require an update
            if (Parent is Folder folder)
                folder.CalculateRenderProperties();

            OnRenderPropertiesUpdated();
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;

            foreach (ProfileElement profileElement in Children)
                profileElement.Dispose();

            _folderBitmap?.Dispose();
            base.Dispose(disposing);
        }

        internal override void Load()
        {
            _expandedPropertyGroups.AddRange(FolderEntity.ExpandedPropertyGroups);

            // Load child folders
            foreach (FolderEntity childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Folder(Profile, this, childFolder));
            // Load child layers
            foreach (LayerEntity childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Layer(Profile, this, childLayer));

            // Ensure order integrity, should be unnecessary but no one is perfect specially me
            ChildrenList = ChildrenList.OrderBy(c => c.Order).ToList();
            for (int index = 0; index < ChildrenList.Count; index++)
                ChildrenList[index].Order = index + 1;

            LoadRenderElement();
        }

        internal override void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;
            FolderEntity.Enabled = Enabled;

            FolderEntity.ProfileId = Profile.EntityId;
            FolderEntity.ExpandedPropertyGroups.Clear();
            FolderEntity.ExpandedPropertyGroups.AddRange(_expandedPropertyGroups);

            SaveRenderElement();
        }

        #region Events

        public event EventHandler RenderPropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}