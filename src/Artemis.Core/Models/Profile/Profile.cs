using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Profile : ProfileElement
    {
        private bool _isActivated;

        internal Profile(ProfileModule module, string name)
        {
            ProfileEntity = new ProfileEntity();
            EntityId = Guid.NewGuid();

            Module = module;
            Name = name;
            UndoStack = new Stack<string>();
            RedoStack = new Stack<string>();

            AddChild(new Folder(this, this, "Root folder"));
            ApplyToEntity();
        }

        internal Profile(ProfileModule module, ProfileEntity profileEntity)
        {
            ProfileEntity = profileEntity;
            EntityId = profileEntity.Id;

            Module = module;
            UndoStack = new Stack<string>();
            RedoStack = new Stack<string>();

            ApplyToProfile();
        }

        public ProfileModule Module { get; }

        public bool IsActivated
        {
            get => _isActivated;
            private set => SetAndNotify(ref _isActivated, value);
        }

        internal ProfileEntity ProfileEntity { get; set; }

        internal Stack<string> UndoStack { get; set; }
        internal Stack<string> RedoStack { get; set; }

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

        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            lock (this)
            {
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot render inactive profile: {this}");

                foreach (var profileElement in Children)
                    profileElement.Render(deltaTime, canvas, canvasInfo);
            }
        }

        public Folder GetRootFolder()
        {
            return (Folder) Children.Single();
        }

        public void ApplyToProfile()
        {
            Name = ProfileEntity.Name;

            lock (ChildrenList)
            {
                foreach (var folder in GetAllFolders())
                    folder.Deactivate();
                foreach (var layer in GetAllLayers())
                    layer.Deactivate();

                ChildrenList.Clear();
                // Populate the profile starting at the root, the rest is populated recursively
                var rootFolder = ProfileEntity.Folders.FirstOrDefault(f => f.ParentId == EntityId);
                if (rootFolder == null)
                    AddChild(new Folder(this, this, "Root folder"));
                else
                    AddChild(new Folder(this, this, rootFolder));
            }
        }

        public override string ToString()
        {
            return $"[Profile] {nameof(Name)}: {Name}, {nameof(IsActivated)}: {IsActivated}, {nameof(Module)}: {Module}";
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            Deactivate();
            foreach (var profileElement in Children)
                profileElement.Dispose();
        }

        internal override void ApplyToEntity()
        {
            ProfileEntity.Id = EntityId;
            ProfileEntity.PluginGuid = Module.PluginInfo.Guid;
            ProfileEntity.Name = Name;
            ProfileEntity.IsActive = IsActivated;

            foreach (var profileElement in Children)
                profileElement.ApplyToEntity();

            ProfileEntity.Folders.Clear();
            ProfileEntity.Folders.AddRange(GetAllFolders().Select(f => f.FolderEntity));

            ProfileEntity.Layers.Clear();
            ProfileEntity.Layers.AddRange(GetAllLayers().Select(f => f.LayerEntity));
        }

        internal void Activate(ArtemisSurface surface)
        {
            lock (this)
            {
                if (IsActivated)
                    return;

                PopulateLeds(surface);
                OnActivated();
                IsActivated = true;
            }
        }

        internal void Deactivate()
        {
            lock (this)
            {
                if (!IsActivated)
                    return;

                foreach (var folder in GetAllFolders())
                    folder.Deactivate();
                foreach (var layer in GetAllLayers())
                    layer.Deactivate();

                IsActivated = false;
                OnDeactivated();
            }
        }

        internal void PopulateLeds(ArtemisSurface surface)
        {
            foreach (var layer in GetAllLayers())
                layer.PopulateLeds(surface);
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