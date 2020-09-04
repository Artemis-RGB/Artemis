using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core
{
    public sealed class Folder : RenderProfileElement
    {
        private SKBitmap _folderBitmap;

        public Folder(Profile profile, ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            Enabled = true;
            DisplayContinuously = true;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();
            ApplyRenderElementDefaults();
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
            _expandedPropertyGroups.AddRange(folderEntity.ExpandedPropertyGroups);

            // Load child folders
            foreach (var childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Folder(profile, this, childFolder));
            // Load child layers
            foreach (var childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Layer(profile, this, childLayer));

            // Ensure order integrity, should be unnecessary but no one is perfect specially me
            ChildrenList = ChildrenList.OrderBy(c => c.Order).ToList();
            for (var index = 0; index < ChildrenList.Count; index++)
                ChildrenList[index].Order = index + 1;

            ApplyRenderElementEntity();
        }

        internal FolderEntity FolderEntity { get; set; }
        internal override RenderElementEntity RenderElementEntity => FolderEntity;
        public bool IsRootFolder => Parent == Profile;

        public override void Update(double deltaTime)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled)
                return;

            UpdateDisplayCondition();

            // Update the layer timeline, this will give us a new delta time which could be negative in case the main segment wrapped back
            // to it's start
            UpdateTimeline(deltaTime);

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update(deltaTime);
                baseLayerEffect.Update(deltaTime);
            }

            // Iterate the children in reverse because that's how they must be rendered too
            for (var index = Children.Count - 1; index > -1; index--)
            {
                var profileElement = Children[index];
                profileElement.Update(deltaTime);
            }
        }

        public override void OverrideProgress(TimeSpan timeOverride, bool stickToMainSegment)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled)
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

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
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

            using var folderPath = new SKPath(Path);
            using var folderCanvas = new SKCanvas(_folderBitmap);
            using var folderPaint = new SKPaint();
            folderCanvas.Clear();

            folderPath.Transform(SKMatrix.MakeTranslation(folderPath.Bounds.Left * -1, folderPath.Bounds.Top * -1));

            var targetLocation = Path.Bounds.Location;
            if (Parent is Folder parentFolder)
                targetLocation -= parentFolder.Path.Bounds.Location;

            canvas.Save();

            using var clipPath = new SKPath(folderPath);
            clipPath.Transform(SKMatrix.MakeTranslation(targetLocation.X, targetLocation.Y));
            canvas.ClipPath(clipPath);

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PreProcess(folderCanvas, _folderBitmap.Info, folderPath, folderPaint);

            // No point rendering if the alpha was set to zero by one of the effects
            if (folderPaint.Color.Alpha == 0)
                return;

            // Iterate the children in reverse because the first layer must be rendered last to end up on top
            for (var index = Children.Count - 1; index > -1; index--)
            {
                folderCanvas.Save();
                var profileElement = Children[index];
                profileElement.Render(deltaTime, folderCanvas, _folderBitmap.Info);
                folderCanvas.Restore();
            }

            // If required, apply the opacity override of the module to the root folder
            if (IsRootFolder && Profile.Module.OpacityOverride < 1)
            {
                var multiplier = Easings.SineEaseInOut(Profile.Module.OpacityOverride);
                folderPaint.Color = folderPaint.Color.WithAlpha((byte) (folderPaint.Color.Alpha * multiplier));
            }

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PostProcess(canvas, canvasInfo, folderPath, folderPaint);
            canvas.DrawBitmap(_folderBitmap, targetLocation, folderPaint);

            canvas.Restore();
        }

        /// <summary>
        ///     Adds a new folder to the bottom of this folder
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Folder AddFolder(string name)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            var folder = new Folder(Profile, this, name) {Order = Children.LastOrDefault()?.Order ?? 1};
            AddChild(folder);
            return folder;
        }

        /// <inheritdoc />
        public override void AddChild(ProfileElement child, int? order = null)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            base.AddChild(child, order);
            CalculateRenderProperties();
        }

        /// <inheritdoc />
        public override void RemoveChild(ProfileElement child)
        {
            if (_disposed)
                throw new ObjectDisposedException("Folder");

            base.RemoveChild(child);
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

            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var child in Children)
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
            if (!disposing)
                return;

            foreach (var baseLayerEffect in LayerEffects)
                baseLayerEffect.Dispose();
            _layerEffects.Clear();

            foreach (var profileElement in Children)
                profileElement.Dispose();
            ChildrenList.Clear();

            _folderBitmap?.Dispose();
            _folderBitmap = null;

            Profile = null;
            _disposed = true;
        }


        internal override void ApplyToEntity()
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

            ApplyRenderElementToEntity();

            // Conditions
            RenderElementEntity.RootDisplayCondition = DisplayConditionGroup?.Entity;
            DisplayConditionGroup?.ApplyToEntity();
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