using System.Collections.Generic;
using Artemis.Core.ProfileElements.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements
{
    public class Folder : IProfileElement
    {
        public Folder(Profile profile)
        {
            Profile = profile;
            Children = new List<IProfileElement>();
        }

        public Profile Profile { get; }
        public List<IProfileElement> Children { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }

        public void Update(double deltaTime)
        {
            // Folders don't update but their children do
            foreach (var profileElement in Children)
                profileElement.Update(deltaTime);
        }

        public void Render(double deltaTime, RGBSurface surface)
        {
            // Folders don't render but their children do
            foreach (var profileElement in Children)
                profileElement.Render(deltaTime, surface);
        }

        public static Folder FromFolderEntity(Profile profile, FolderEntity folderEntity, IPluginService pluginService)
        {
            var folder = new Folder(profile)
            {
                Name = folderEntity.Name,
                Order = folderEntity.Order
            };

            // Load child folders
            foreach (var childFolder in folderEntity.Folders)
                folder.Children.Add(FromFolderEntity(profile, childFolder, pluginService));
            // Load child layers
            foreach (var childLayer in folderEntity.Layers)
                folder.Children.Add(Layer.FromLayerEntity(profile, childLayer, pluginService));

            return folder;
        }

        public override string ToString()
        {
            return $"{nameof(Profile)}: {Profile}, {nameof(Order)}: {Order}, {nameof(Name)}: {Name}";
        }
    }
}