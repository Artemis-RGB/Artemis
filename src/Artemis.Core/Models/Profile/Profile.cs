using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a profile containing folders and layers
/// </summary>
public sealed class Profile : ProfileElement
{
    private readonly object _lock = new();
    private bool _isFreshImport;

    internal Profile(ProfileConfiguration configuration, ProfileEntity profileEntity) : base(null!)
    {
        Opacity = 0d;
        ShouldDisplay = true;
        Configuration = configuration;
        Profile = this;
        ProfileEntity = profileEntity;
        EntityId = profileEntity.Id;

        Exceptions = new List<Exception>();

        Load();
    }

    /// <summary>
    ///     Gets the profile configuration of this profile
    /// </summary>
    public ProfileConfiguration Configuration { get; }
    
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

    internal List<Exception> Exceptions { get; }

    internal bool ShouldDisplay { get; set; }

    internal double Opacity { get; private set; }

    /// <inheritdoc />
    public override void Update(double deltaTime)
    {
        lock (_lock)
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            foreach (ProfileElement profileElement in Children)
                profileElement.Update(deltaTime);

            const double OPACITY_PER_SECOND = 1;

            if (ShouldDisplay && Opacity < 1)
                Opacity = Math.Clamp(Opacity + OPACITY_PER_SECOND * deltaTime, 0d, 1d);
            if (!ShouldDisplay && Opacity > 0)
                Opacity = Math.Clamp(Opacity - OPACITY_PER_SECOND * deltaTime, 0d, 1d);
        }
    }

    /// <inheritdoc />
    public override void Render(SKCanvas canvas, SKPointI basePosition, ProfileElement? editorFocus)
    {
        lock (_lock)
        {
            if (Disposed)
                throw new ObjectDisposedException("Profile");

            SKPaint? opacityPaint = null;
            bool applyOpacityLayer = Configuration.FadeInAndOut && Opacity < 1;

            if (applyOpacityLayer)
            {
                opacityPaint = new SKPaint();
                opacityPaint.Color = new SKColor(0, 0, 0, (byte) (255d * Easings.CubicEaseInOut(Opacity)));
                canvas.SaveLayer(opacityPaint);
            }

            foreach (ProfileElement profileElement in Children)
                profileElement.Render(canvas, basePosition, editorFocus);

            if (applyOpacityLayer)
            {
                canvas.Restore();
                opacityPaint?.Dispose();
            }

            if (!Exceptions.Any())
                return;

            List<Exception> exceptions = new(Exceptions);
            Exceptions.Clear();
            throw new AggregateException($"One or more exceptions while rendering profile {Name}", exceptions);
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

    /// <inheritdoc />
    public override IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        return GetRootFolder().GetFeatureDependencies();
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

    #region Overrides of BreakableModel

    /// <inheritdoc />
    public override IEnumerable<IBreakableModel> GetBrokenHierarchy()
    {
        return GetAllRenderElements().SelectMany(folders => folders.GetBrokenHierarchy());
    }

    #endregion

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        
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
                AddChild(new Folder(this, "Root folder"));
            else
                AddChild(new Folder(this, this, rootFolder));
        }

        // Load node scripts last since they may rely on the profile structure being in place
        foreach (RenderProfileElement renderProfileElement in GetAllRenderElements())
            renderProfileElement.LoadNodeScript();
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
    }
}