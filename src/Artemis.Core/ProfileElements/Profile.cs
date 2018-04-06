using System.Collections.Generic;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.ProfileElements
{
    public class Profile : IProfileElement
    {
        private Profile(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public int Order { get; set; }
        public string Name { get; set; }
        public PluginInfo PluginInfo { get; }
        public List<IProfileElement> Children { get; set; }

        public void Update()
        {
            foreach (var profileElement in Children)
                profileElement.Update();
        }

        public void Render(IRGBDevice rgbDevice)
        {
            foreach (var profileElement in Children)
                profileElement.Render(rgbDevice);
        }

        public static Profile FromProfileEntity(PluginInfo pluginInfo, ProfileEntity profileEntity, IPluginService pluginService)
        {
            var profile = new Profile(pluginInfo) {Name = profileEntity.Name};

            // Populate the profile starting at the root, the rest is populated recursively
            profile.Children.Add(Folder.FromFolderEntity(profileEntity.RootFolder, pluginService));
            
            return profile;
        }
    }
}