using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Folder : ProfileElement
    {
        private readonly List<BaseLayerEffect> _layerEffects;

        public Folder(Profile profile, ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
            _layerEffects = new List<BaseLayerEffect>();
        }

        internal Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity)
        {
            FolderEntity = folderEntity;
            EntityId = folderEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = folderEntity.Name;
            Order = folderEntity.Order;
            _layerEffects = new List<BaseLayerEffect>();

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

        /// <summary>
        ///     Gets a read-only collection of the layer effects on this layer
        /// </summary>
        public ReadOnlyCollection<BaseLayerEffect> LayerEffects => _layerEffects.AsReadOnly();

        public override void Update(double deltaTime)
        {
            // Iterate the children in reverse because that's how they must be rendered too
            for (var index = Children.Count - 1; index > -1; index--)
            {
                var profileElement = Children[index];
                profileElement.Update(deltaTime);
            }
        }

        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            // Iterate the children in reverse because the first layer must be rendered last to end up on top
            for (var index = Children.Count - 1; index > -1; index--)
            {
                var profileElement = Children[index];
                profileElement.Render(deltaTime, canvas, canvasInfo);
            }
        }

        public Folder AddFolder(string name)
        {
            var folder = new Folder(Profile, this, name) {Order = Children.LastOrDefault()?.Order ?? 1};
            AddChild(folder);
            return folder;
        }

        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        internal override void ApplyToEntity()
        {
            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;

            FolderEntity.ProfileId = Profile.EntityId;

            // TODO: conditions
        }
    }
}