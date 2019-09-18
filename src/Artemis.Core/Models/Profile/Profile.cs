using System;
using System.Collections.Generic;
using System.Drawing;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.Models.Profile
{
    public class Profile : IProfileElement
    {
        private Profile(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public PluginInfo PluginInfo { get; }
        public bool IsActivated { get; private set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public List<IProfileElement> Children { get; set; }

        public void Update(double deltaTime)
        {
            lock (this)
            {
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot update inactive profile: {this}");

                foreach (var profileElement in Children)
                    profileElement.Update(deltaTime);
            }
        }

        public void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            lock (this)
            {
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot render inactive profile: {this}");

                foreach (var profileElement in Children)
                    profileElement.Render(deltaTime, surface, graphics);
            }
        }

        public static Profile FromProfileEntity(PluginInfo pluginInfo, ProfileEntity profileEntity, IPluginService pluginService)
        {
            var profile = new Profile(pluginInfo) {Name = profileEntity.Name};
            lock (profile)
            {
                // Populate the profile starting at the root, the rest is populated recursively
                profile.Children.Add(Folder.FromFolderEntity(profile, profileEntity.RootFolder, pluginService));

                return profile;
            }
        }

        public void Activate()
        {
            lock (this)
            {
                if (IsActivated) return;

                OnActivated();
                IsActivated = true;
            }
        }

        public void Deactivate()
        {
            lock (this)
            {
                if (!IsActivated) return;

                IsActivated = false;
                OnDeactivated();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Order)}: {Order}, {nameof(Name)}: {Name}, {nameof(PluginInfo)}: {PluginInfo}";
        }

        #region Events

        /// <summary>
        ///     Occurs when the profile is being activated.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        ///     Occurs when the profile is being deactivated.
        /// </summary>
        public event EventHandler Deactivated;

        private void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeactivated()
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}