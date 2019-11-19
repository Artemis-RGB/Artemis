using System.Collections.Generic;
using System.Drawing;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;

namespace Artemis.Core.Models.Profile
{
    public class Folder : ProfileElement
    {
        public Folder(Profile profile, Folder folder, string name)
        {
            FolderEntity = new FolderEntity();
            Guid = System.Guid.NewGuid().ToString();

            Profile = profile;
            ParentFolder = folder;
            Name = name;
            Children = new List<ProfileElement>();
        }

        public Folder(Profile profile, Folder folder, FolderEntity folderEntity, IPluginService pluginService)
        {
            FolderEntity = folderEntity;
            Guid = folderEntity.Guid;

            Profile = profile;
            ParentFolder = folder;
            Children = new List<ProfileElement>();

            // Load child folders
            foreach (var childFolder in folderEntity.Folders)
                folder.Children.Add(new Folder(profile, this, childFolder, pluginService));
            // Load child layers
            foreach (var childLayer in folderEntity.Layers)
                folder.Children.Add(new Layer(profile, this, childLayer, pluginService));
        }

        internal FolderEntity FolderEntity { get; set; }
        internal string Guid { get; set; }

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
            FolderEntity.Guid = Guid;
            FolderEntity.Order = Order;
            FolderEntity.Name = Name;

            foreach (var profileElement in Children)
            {
                profileElement.ApplyToEntity();
                // Add missing children
                if (profileElement is Folder folder)
                {
                    // TODO
                }
                else if (profileElement is Layer layer)
                {
                    // TODO
                }

                // Remove extra childen
            }
        }

        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}