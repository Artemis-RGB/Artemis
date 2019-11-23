using System;
using System.Drawing;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile
{
    public sealed class Profile : ProfileElement
    {
        internal Profile(PluginInfo pluginInfo, string name)
        {
            ProfileEntity = new ProfileEntity();
            EntityId = Guid.NewGuid();

            PluginInfo = pluginInfo;
            Name = name;

            AddChild(new Folder(this, null, "Root folder"));
            ApplyToEntity();
        }

        internal Profile(PluginInfo pluginInfo, ProfileEntity profileEntity, IPluginService pluginService)
        {
            ProfileEntity = profileEntity;
            EntityId = profileEntity.Id;

            PluginInfo = pluginInfo;
            Name = profileEntity.Name;

            // Populate the profile starting at the root, the rest is populated recursively
            var rootFolder = profileEntity.Folders.FirstOrDefault(f => f.ParentId == new Guid());
            if (rootFolder == null)
                AddChild(new Folder(this, null, "Root folder"));
            else
                AddChild(new Folder(this, null, rootFolder, pluginService));
        }

        public PluginInfo PluginInfo { get; }
        public bool IsActivated { get; private set; }

        internal ProfileEntity ProfileEntity { get; set; }

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
            ProfileEntity.Id = EntityId;
            ProfileEntity.PluginGuid = PluginInfo.Guid;
            ProfileEntity.Name = Name;
            ProfileEntity.IsActive = IsActivated;

            foreach (var profileElement in Children)
                profileElement.ApplyToEntity();

            ProfileEntity.Folders.Clear();
            ProfileEntity.Folders.AddRange(GetAllFolders().Select(f => f.FolderEntity));

            ProfileEntity.Layers.Clear();
            ProfileEntity.Layers.AddRange(GetAllLayers().Select(f => f.LayerEntity));
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

        public void ApplySurface(Surface.Surface surface)
        {
            foreach (var layer in GetAllLayers())
                layer.ApplySurface(surface);
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