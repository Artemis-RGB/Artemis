using System.Collections.Generic;
using Artemis.Core.ProfileElements.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements
{
    public class Folder : IProfileElement
    {
        public Folder()
        {
            Children = new List<IProfileElement>();
        }

        public List<IProfileElement> Children { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }

        public void Update()
        {
            // Folders don't update but their children do
            foreach (var profileElement in Children)
                profileElement.Update();
        }

        public void Render(IRGBDevice rgbDevice)
        {
            // Folders don't render but their children do
            foreach (var profileElement in Children)
                profileElement.Render(rgbDevice);
        }

        public static Folder FromFolderEntity(FolderEntity folderEntity, IPluginService pluginService)
        {
            var folder = new Folder
            {
                Name = folderEntity.Name,
                Order = folderEntity.Order
            };
            
            // Load child folders
            foreach (var childFolder in folderEntity.Folders)
                folder.Children.Add(FromFolderEntity(childFolder, pluginService));
            // Load child layers
            foreach (var childLayer in folderEntity.Layers)
                folder.Children.Add(Layer.FromLayerEntity(childLayer, pluginService));

            return folder;
        }
    }
}