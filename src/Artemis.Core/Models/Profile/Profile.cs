using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a profile containing folders and layers
    /// </summary>
    public sealed class Profile : ProfileElement
    {
        private readonly object _lock = new object();
        private bool _isActivated;

        internal Profile(ProfileModule module, string name) : base(null!)
        {
            ProfileEntity = new ProfileEntity();
            EntityId = Guid.NewGuid();

            Profile = this;
            Module = module;
            Name = name;
            UndoStack = new Stack<string>();
            RedoStack = new Stack<string>();

            Folder _ = new Folder(this, "Root folder");
            Save();
        }

        internal Profile(ProfileModule module, ProfileEntity profileEntity) : base(null!)
        {
            Profile = this;
            ProfileEntity = profileEntity;
            EntityId = profileEntity.Id;

            Module = module;
            UndoStack = new Stack<string>();
            RedoStack = new Stack<string>();

            Load();
        }

        /// <summary>
        ///     Gets the module backing this profile
        /// </summary>
        public ProfileModule Module { get; }

        /// <summary>
        ///     Gets a boolean indicating whether this profile is activated
        /// </summary>
        public bool IsActivated
        {
            get => _isActivated;
            private set => SetAndNotify(ref _isActivated, value);
        }

        /// <summary>
        ///     Gets the profile entity this profile uses for persistent storage
        /// </summary>
        public ProfileEntity ProfileEntity { get; internal set; }

        internal Stack<string> UndoStack { get; set; }
        internal Stack<string> RedoStack { get; set; }

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
            lock (_lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException("Profile");
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot update inactive profile: {this}");

                foreach (ProfileElement profileElement in Children)
                    profileElement.Update(deltaTime);
            }
        }

        /// <inheritdoc />
        public override void Render(SKCanvas canvas)
        {
            lock (_lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException("Profile");
                if (!IsActivated)
                    throw new ArtemisCoreException($"Cannot render inactive profile: {this}");

                foreach (ProfileElement profileElement in Children)
                    profileElement.Render(canvas);
            }
        }

        /// <inheritdoc />
        public override void Reset()
        {
            foreach (ProfileElement child in Children)
                child.Reset();
        }

        /// <summary>
        ///     Retrieves the root folder of this profile
        /// </summary>
        /// <returns>The root folder of the profile</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public Folder GetRootFolder()
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            return (Folder) Children.Single();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Profile] {nameof(Name)}: {Name}, {nameof(IsActivated)}: {IsActivated}, {nameof(Module)}: {Module}";
        }

        /// <summary>
        ///     Populates all the LEDs on the elements in this profile
        /// </summary>
        /// <param name="surface">The currently active surface that contains the LEDs</param>
        public void PopulateLeds(ArtemisSurface surface)
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            foreach (Layer layer in GetAllLayers())
                layer.PopulateLeds(surface);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            OnDeactivating();

            foreach (ProfileElement profileElement in Children)
                profileElement.Dispose();
            ChildrenList.Clear();

            IsActivated = false;
            Disposed = true;
        }

        internal override void Load()
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            Name = ProfileEntity.Name;

            lock (ChildrenList)
            {
                // Remove the old profile tree
                foreach (ProfileElement profileElement in Children)
                    profileElement.Dispose();
                ChildrenList.Clear();

                // Populate the profile starting at the root, the rest is populated recursively
                FolderEntity? rootFolder = ProfileEntity.Folders.FirstOrDefault(f => f.ParentId == EntityId);
                if (rootFolder == null)
                {
                    Folder _ = new Folder(this, "Root folder");
                }
                else
                {
                    AddChild(new Folder(this, this, rootFolder));
                }
            }
        }

        internal override void Save()
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            ProfileEntity.Id = EntityId;
            ProfileEntity.ModuleId = Module.Id;
            ProfileEntity.Name = Name;
            ProfileEntity.IsActive = IsActivated;

            foreach (ProfileElement profileElement in Children)
                profileElement.Save();

            ProfileEntity.Folders.Clear();
            ProfileEntity.Folders.AddRange(GetAllFolders().Select(f => f.FolderEntity));

            ProfileEntity.Layers.Clear();
            ProfileEntity.Layers.AddRange(GetAllLayers().Select(f => f.LayerEntity));
        }

        internal void Activate(ArtemisSurface surface)
        {
            lock (_lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException("Profile");
                if (IsActivated)
                    return;

                PopulateLeds(surface);
                OnActivated();
                IsActivated = true;
            }
        }

        #region Events

        /// <summary>
        ///     Occurs when the profile has been activated.
        /// </summary>
        public event EventHandler? Activated;

        /// <summary>
        ///     Occurs when the profile is being deactivated.
        /// </summary>
        public event EventHandler? Deactivated;

        private void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeactivating()
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}