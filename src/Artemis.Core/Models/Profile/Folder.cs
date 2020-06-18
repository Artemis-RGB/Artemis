using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Folder : EffectProfileElement
    {
        public Folder(Profile profile, ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            Enabled = true;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();
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

            // TODO: Load conditions

            // Load child folders
            foreach (var childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                _children.Add(new Folder(profile, this, childFolder));
            // Load child layers
            foreach (var childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                _children.Add(new Layer(profile, this, childLayer));

            // Ensure order integrity, should be unnecessary but no one is perfect specially me
            _children = _children.OrderBy(c => c.Order).ToList();
            for (var index = 0; index < _children.Count; index++)
                _children[index].Order = index + 1;
        }

        internal FolderEntity FolderEntity { get; set; }
        internal override PropertiesEntity PropertiesEntity => FolderEntity;
        internal override EffectsEntity EffectsEntity => FolderEntity;

        public override void Update(double deltaTime)
        {
            if (!Enabled)
                return;

            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.Update(deltaTime);

            // Iterate the children in reverse because that's how they must be rendered too
            for (var index = Children.Count - 1; index > -1; index--)
            {
                var profileElement = Children[index];
                profileElement.Update(deltaTime);
            }
        }

        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo, SKPaint paint)
        {
            if (!Enabled)
                return;

            if (Path == null)
                return;

            canvas.Save();
            canvas.ClipPath(Path);

            // Clone the paint so that any changes are confined to the current group
            var groupPaint = paint.Clone();

            // Pre-processing only affects other pre-processors and the brushes
            canvas.Save();
            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.InternalPreProcess(canvas, canvasInfo, new SKPath(Path), groupPaint);

            // Iterate the children in reverse because the first layer must be rendered last to end up on top
            for (var index = Children.Count - 1; index > -1; index--)
            {
                var profileElement = Children[index];
                profileElement.Render(deltaTime, canvas, canvasInfo, groupPaint);
            }

            // Restore the canvas as to not be affected by pre-processors
            canvas.Restore();
            foreach (var baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.InternalPostProcess(canvas, canvasInfo, new SKPath(Path), groupPaint);

            canvas.Restore();
        }

        /// <summary>
        ///     Adds a new folder to the bottom of this folder
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Folder AddFolder(string name)
        {
            var folder = new Folder(Profile, this, name) {Order = Children.LastOrDefault()?.Order ?? 1};
            AddChild(folder);
            return folder;
        }

        /// <inheritdoc />
        public override void AddChild(ProfileElement child, int? order = null)
        {
            base.AddChild(child, order);
            CalculateRenderProperties();
        }

        /// <inheritdoc />
        public override void RemoveChild(ProfileElement child)
        {
            base.RemoveChild(child);
            CalculateRenderProperties();
        }

        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        public void CalculateRenderProperties()
        {
            var path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (var child in Children)
            {
                if (child is EffectProfileElement effectChild && effectChild.Path != null)
                    path.AddPath(effectChild.Path);
            }

            Path = path;

            // Folder render properties are based on child paths and thus require an update
            if (Parent is Folder folder)
                folder.CalculateRenderProperties();

            OnRenderPropertiesUpdated();
        }

        internal override void ApplyToEntity()
        {
            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;
            FolderEntity.Enabled = Enabled;

            FolderEntity.ProfileId = Profile.EntityId;

            ApplyLayerEffectsToEntity();

            // TODO: conditions
        }

        #region Events

        public event EventHandler RenderPropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        internal void Deactivate()
        {
            var layerEffects = new List<BaseLayerEffect>(LayerEffects);
            foreach (var baseLayerEffect in layerEffects)
                DeactivateLayerEffect(baseLayerEffect);
        }
    }
}