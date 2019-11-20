using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile
{
    public sealed class Folder : ProfileElement
    {
        public Folder(Profile profile, Folder folder, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            ParentFolder = folder;
            Name = name;
            Children = new List<ProfileElement>();
        }

        public Folder(Profile profile, Folder folder, FolderEntity folderEntity, IPluginService pluginService)
        {
            FolderEntity = folderEntity;
            EntityId = folderEntity.Id;

            Profile = profile;
            ParentFolder = folder;
            Children = new List<ProfileElement>();

            // Load child folders
            foreach (var childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                folder.Children.Add(new Folder(profile, this, childFolder, pluginService));
            // Load child layers
            foreach (var childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                folder.Children.Add(new Layer(profile, this, childLayer, pluginService));
        }

        internal FolderEntity FolderEntity { get; set; }
        internal Guid EntityId { get; set; }

        public Profile Profile { get; }
        public Folder ParentFolder { get; }


        public override void Update(double deltaTime)
        {
            // Folders don't update but their children do
            foreach (var profileElement in Children)
                profileElement.Update(deltaTime);
        }

        public override void Render(double deltaTime, Surface.Surface surface, Graphics graphics)
        {
            // Folders don't render but their children do
            foreach (var profileElement in Children)
                profileElement.Render(deltaTime, surface, graphics);
        }

        internal override void ApplyToEntity()
        {
            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = ParentFolder?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;

            FolderEntity.ProfileId = Profile.EntityId;

            // TODO: conditions
        }

        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}