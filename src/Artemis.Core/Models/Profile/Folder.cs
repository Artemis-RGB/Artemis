using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        ///     Creates a new instance of the <see cref="Folder" /> class and adds itself to the child collection of the provided
        ///     <paramref name="parent" />
        /// </summary>
        /// <param name="parent">The parent of the folder</param>
        /// <param name="name">The name of the folder</param>
        public Folder(ProfileElement parent, string name) : base(parent.Profile)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Profile = Parent.Profile;
            Name = name;

            Parent.AddChild(this);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Folder" /> class based on the provided folder entity
        /// </summary>
        /// <param name="profile">The profile the folder belongs to</param>
        /// <param name="parent">The parent of the folder</param>
        /// <param name="folderEntity">The entity of the folder</param>
        public Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity) : base(parent.Profile)
        {
            FolderEntity = folderEntity;
            EntityId = folderEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = folderEntity.Name;
            Suspended = folderEntity.Suspended;
            Order = folderEntity.Order;

            Load();
        }

        /// <summary>
        ///     Gets a boolean indicating whether this folder is at the root of the profile tree
        /// </summary>
        public bool IsRootFolder => Parent == Profile;

        /// <summary>
        ///     Gets the folder entity this folder uses for persistent storage
        /// </summary>
        public FolderEntity FolderEntity { get; internal set; }

        /// <inheritdoc />
        public override bool ShouldBeEnabled => !Suspended && DisplayConditionMet && !Timeline.IsFinished;

        internal override RenderElementEntity RenderElementEntity => FolderEntity;

        /// <inheritdoc />
        public override List<ILayerProperty> GetAllLayerProperties()
        {
            List<ILayerProperty> result = new();
            foreach (BaseLayerEffect layerEffect in LayerEffects)
                if (layerEffect.BaseProperties != null)
                    result.AddRange(layerEffect.BaseProperties.GetAllLayerProperties());

            return result;
        }

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            UpdateDisplayCondition();
            UpdateTimeline(deltaTime);

            if (ShouldBeEnabled)
                Enable();
            else if (Timeline.IsFinished)
                Disable();

            foreach (ProfileElement child in Children)
                child.Update(deltaTime);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            UpdateDisplayCondition();

            if (DisplayConditionMet)
                Timeline.JumpToStart();
            else
                Timeline.JumpToEnd();
        }

        /// <inheritdoc />
        public override void AddChild(ProfileElement child, int? order = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            base.AddChild(child, order);
            CalculateRenderProperties();
        }

        /// <inheritdoc />
        public override void RemoveChild(ProfileElement child)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            base.RemoveChild(child);
            CalculateRenderProperties();
        }

        /// <summary>
        ///     Creates a deep copy of the folder
        /// </summary>
        /// <returns>The newly created copy</returns>
        public Folder CreateCopy()
        {
            if (Parent == null)
                throw new ArtemisCoreException("Cannot create a copy of a folder without a parent");

            FolderEntity entityCopy = CoreJson.DeserializeObject<FolderEntity>(CoreJson.SerializeObject(FolderEntity, true), true)!;
            entityCopy.Id = Guid.NewGuid();
            entityCopy.Name += " - Copy";

            // TODO Children

            return new Folder(Profile, Parent, entityCopy);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        internal void CalculateRenderProperties()
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            SKPath path = new() {FillType = SKPathFillType.Winding};
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

        #region Rendering

        /// <inheritdoc />
        public override void Render(SKCanvas canvas, SKPointI basePosition)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            // Ensure the folder is ready
            if (!Enabled || Path == null)
                return;

            // No point rendering if all children are disabled
            if (!Children.Any(c => c is RenderProfileElement {Enabled: true}))
                return;

            lock (Timeline)
            {
                foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => !e.Suspended))
                {
                    baseLayerEffect.BaseProperties?.Update(Timeline);
                    baseLayerEffect.Update(Timeline.Delta.TotalSeconds);
                }

                SKPaint layerPaint = new() {FilterQuality = SKFilterQuality.Low};
                try
                {
                    SKRectI rendererBounds = SKRectI.Create(0, 0, Bounds.Width, Bounds.Height);
                    foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => !e.Suspended))
                        baseLayerEffect.PreProcess(canvas, rendererBounds, layerPaint);

                    // No point rendering if the alpha was set to zero by one of the effects
                    if (layerPaint.Color.Alpha == 0)
                        return;

                    canvas.SaveLayer(layerPaint);
                    canvas.Translate(Bounds.Left - basePosition.X, Bounds.Top - basePosition.Y);
                    
                    // Iterate the children in reverse because the first layer must be rendered last to end up on top
                    for (int index = Children.Count - 1; index > -1; index--)
                        Children[index].Render(canvas, new SKPointI(Bounds.Left, Bounds.Top));

                    foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => !e.Suspended))
                        baseLayerEffect.PostProcess(canvas, rendererBounds, layerPaint);
                }
                finally
                {
                    canvas.Restore();
                    layerPaint.DisposeSelfAndProperties();
                }

                Timeline.ClearDelta();
            }
        }

        #endregion

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            Disposed = true;

            Disable();
            foreach (ProfileElement profileElement in Children)
                profileElement.Dispose();

            base.Dispose(disposing);
        }

        internal override void Load()
        {
            ExpandedPropertyGroups.AddRange(FolderEntity.ExpandedPropertyGroups);
            Reset();

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
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;
            FolderEntity.Suspended = Suspended;

            FolderEntity.ProfileId = Profile.EntityId;
            FolderEntity.ExpandedPropertyGroups.Clear();
            FolderEntity.ExpandedPropertyGroups.AddRange(ExpandedPropertyGroups);

            SaveRenderElement();
        }

        /// <inheritdoc />
        public override void Enable()
        {
            if (Enabled)
                return;

            Debug.WriteLine($"Enabling {this}");

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
                baseLayerEffect.InternalEnable();

            foreach (ProfileElement profileElement in Children)
            {
                if (profileElement is RenderProfileElement renderProfileElement && renderProfileElement.ShouldBeEnabled)
                    renderProfileElement.Enable();
            }

            Enabled = true;
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (!Enabled)
                return;

            Debug.WriteLine($"Disabled {this}");

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
                baseLayerEffect.InternalDisable();

            foreach (ProfileElement profileElement in Children)
            {
                if (profileElement is RenderProfileElement renderProfileElement)
                    renderProfileElement.Disable();
            }

            Enabled = false;
        }

        #region Events

        /// <summary>
        ///     Occurs when a property affecting the rendering properties of this folder has been updated
        /// </summary>
        public event EventHandler? RenderPropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}