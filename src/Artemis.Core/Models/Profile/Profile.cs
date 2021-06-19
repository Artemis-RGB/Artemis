using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.ScriptingProviders;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a profile containing folders and layers
    /// </summary>
    public sealed class Profile : ProfileElement
    {
        private readonly object _lock = new();
        private bool _isFreshImport;

        internal Profile(ProfileConfiguration configuration, ProfileEntity profileEntity) : base(null!)
        {
            Configuration = configuration;
            Profile = this;
            ProfileEntity = profileEntity;
            EntityId = profileEntity.Id;
            Scripts = new List<ProfileScript>();
            ScriptConfigurations = new List<ScriptConfiguration>();

            UndoStack = new Stack<string>();
            RedoStack = new Stack<string>();

            Load();
        }

        /// <summary>
        ///     Gets the profile configuration of this profile
        /// </summary>
        public ProfileConfiguration Configuration { get; }

        /// <summary>
        ///     Gets a collection of all active scripts assigned to this profile
        /// </summary>
        public List<ProfileScript> Scripts { get; }

        /// <summary>
        ///     Gets a collection of all script configurations assigned to this profile
        /// </summary>
        public List<ScriptConfiguration> ScriptConfigurations { get; }


        /// <summary>
        ///     Gets or sets a boolean indicating whether this profile is freshly imported i.e. no changes have been made to it
        ///     since import
        ///     <para>
        ///         Note: As long as this is <see langword="true" />, profile adaption will be performed on load and any surface
        ///         changes
        ///     </para>
        /// </summary>
        public bool IsFreshImport
        {
            get => _isFreshImport;
            set => SetAndNotify(ref _isFreshImport, value);
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

                foreach (ProfileScript profileScript in Scripts)
                    profileScript.OnProfileUpdating(deltaTime);

                foreach (ProfileElement profileElement in Children)
                    profileElement.Update(deltaTime);

                foreach (ProfileScript profileScript in Scripts)
                    profileScript.OnProfileUpdated(deltaTime);
            }
        }

        /// <inheritdoc />
        public override void Render(SKCanvas canvas, SKPointI basePosition)
        {
            lock (_lock)
            {
                if (Disposed)
                    throw new ObjectDisposedException("Profile");

                foreach (ProfileScript profileScript in Scripts)
                    profileScript.OnProfileRendering(canvas, canvas.LocalClipBounds);

                foreach (ProfileElement profileElement in Children)
                    profileElement.Render(canvas, basePosition);

                foreach (ProfileScript profileScript in Scripts)
                    profileScript.OnProfileRendered(canvas, canvas.LocalClipBounds);
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
            return $"[Profile] {nameof(Name)}: {Name}";
        }

        /// <summary>
        ///     Populates all the LEDs on the elements in this profile
        /// </summary>
        /// <param name="devices">The devices to use while populating LEDs</param>
        public void PopulateLeds(IEnumerable<ArtemisDevice> devices)
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            foreach (Layer layer in GetAllLayers())
                layer.PopulateLeds(devices);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            while (Scripts.Count > 1)
                Scripts[0].Dispose();

            foreach (ProfileElement profileElement in Children)
                profileElement.Dispose();
            ChildrenList.Clear();
            Disposed = true;
        }

        internal override void Load()
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            Name = Configuration.Name;
            IsFreshImport = ProfileEntity.IsFreshImport;

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
                    Folder _ = new(this, "Root folder");
                }
                else
                {
                    AddChild(new Folder(this, this, rootFolder));
                }
            }

            foreach (ScriptConfiguration scriptConfiguration in ScriptConfigurations)
                scriptConfiguration.Script?.Dispose();
            ScriptConfigurations.Clear();
            ScriptConfigurations.AddRange(ProfileEntity.ScriptConfigurations.Select(e => new ScriptConfiguration(e)));
        }

        internal override void Save()
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            ProfileEntity.Id = EntityId;
            ProfileEntity.Name = Configuration.Name;
            ProfileEntity.IsFreshImport = IsFreshImport;

            foreach (ProfileElement profileElement in Children)
                profileElement.Save();

            ProfileEntity.Folders.Clear();
            ProfileEntity.Folders.AddRange(GetAllFolders().Select(f => f.FolderEntity));

            ProfileEntity.Layers.Clear();
            ProfileEntity.Layers.AddRange(GetAllLayers().Select(f => f.LayerEntity));

            ProfileEntity.ScriptConfigurations.Clear();
            foreach (ScriptConfiguration scriptConfiguration in ScriptConfigurations)
            {
                scriptConfiguration.Save();
                ProfileEntity.ScriptConfigurations.Add(scriptConfiguration.Entity);
            }
        }
    }
}