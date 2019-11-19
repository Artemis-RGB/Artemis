using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;

namespace Artemis.Core.Models.Profile
{
    public class Profile : ProfileElement
    {
        internal Profile(PluginInfo pluginInfo, string name)
        {
            ProfileEntity = new ProfileEntity {RootFolder = new FolderEntity()};
            Guid = System.Guid.NewGuid().ToString();

            PluginInfo = pluginInfo;
            Name = name;

            Children = new List<ProfileElement> {new Folder(this, null, "Root folder")};
        }

        internal Profile(PluginInfo pluginInfo, ProfileEntity profileEntity, IPluginService pluginService)
        {
            ProfileEntity = profileEntity;
            Guid = profileEntity.Guid;

            PluginInfo = pluginInfo;
            Name = profileEntity.Name;

            // Populate the profile starting at the root, the rest is populated recursively
            Children = new List<ProfileElement> {new Folder(this, null, profileEntity.RootFolder, pluginService)};
        }

        public PluginInfo PluginInfo { get; }
        public bool IsActivated { get; private set; }

        internal ProfileEntity ProfileEntity { get; set; }
        internal string Guid { get; set; }

        public override void Update(double deltaTime)
        {
            lock (this)
            {
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot update inactive profile: {this}");

                foreach (var profileElement in Children)
                    profileElement.Update(deltaTime);
            }
        }

        public override void Render(double deltaTime, Surface.Surface surface, Graphics graphics)
        {
            lock (this)
            {
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot render inactive profile: {this}");

                foreach (var profileElement in Children)
                    profileElement.Render(deltaTime, surface, graphics);
            }
        }

        internal override void ApplyToEntity()
        {
            ProfileEntity.Guid = Guid;
            ProfileEntity.Name = Name;
            ProfileEntity.IsActive = IsActivated;

            var rootFolder = Children.Single();
        }

        internal void Activate()
        {
            lock (this)
            {
                if (IsActivated) return;

                OnActivated();
                IsActivated = true;
            }
        }

        internal void Deactivate()
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