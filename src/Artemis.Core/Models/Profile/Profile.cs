using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.ScriptingProviders;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a profile containing folders and layers
/// </summary>
public sealed class Profile : ProfileElement
{
    private readonly object _lock = new();
    private readonly ObservableCollection<ScriptConfiguration> _scriptConfigurations;
    private readonly ObservableCollection<ProfileScript> _scripts;
    private bool _isFreshImport;
    private ProfileElement? _lastSelectedProfileElement;

    internal Profile(ProfileConfiguration configuration, ProfileEntity profileEntity) : base(null!)
    {
        _scripts = new ObservableCollection<ProfileScript>();
        _scriptConfigurations = new ObservableCollection<ScriptConfiguration>();
        
        Opacity = 0d;
        ShouldBeEnabled = true;
        Configuration = configuration;
        Profile = this;
        ProfileEntity = profileEntity;
        EntityId = profileEntity.Id;

        Exceptions = new List<Exception>();
        Scripts = new ReadOnlyObservableCollection<ProfileScript>(_scripts);
        ScriptConfigurations = new ReadOnlyObservableCollection<ScriptConfiguration>(_scriptConfigurations);

        Load();
    }

    /// <summary>
    ///     Gets the profile configuration of this profile
    /// </summary>
    public ProfileConfiguration Configuration { get; }

    /// <summary>
    ///     Gets a collection of all active scripts assigned to this profile
    /// </summary>
    public ReadOnlyObservableCollection<ProfileScript> Scripts { get; }

    /// <summary>
    ///     Gets a collection of all script configurations assigned to this profile
    /// </summary>
    public ReadOnlyObservableCollection<ScriptConfiguration> ScriptConfigurations { get; }

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
    ///     Gets or sets the last selected profile element of this profile
    /// </summary>
    public ProfileElement? LastSelectedProfileElement
    {
        get => _lastSelectedProfileElement;
        set => SetAndNotify(ref _lastSelectedProfileElement, value);
    }

    /// <summary>
    ///     Gets the profile entity this profile uses for persistent storage
    /// </summary>
    public ProfileEntity ProfileEntity { get; internal set; }

    internal List<Exception> Exceptions { get; }

    internal bool ShouldBeEnabled { get; private set; }

    internal double Opacity { get; private set; }

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

            const double OPACITY_PER_SECOND = 1;
            
            if (ShouldBeEnabled && Opacity < 1)
                Opacity = Math.Clamp(Opacity + OPACITY_PER_SECOND * deltaTime, 0d, 1d);
            if (!ShouldBeEnabled && Opacity > 0)
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

            foreach (ProfileScript profileScript in Scripts)
                profileScript.OnProfileRendering(canvas, canvas.LocalClipBounds);

            using var opacityPaint = new SKPaint();
            if (Configuration.FadeInAndOut && Opacity != 1)
                opacityPaint.Color = new SKColor(0, 0, 0, (byte)(255d * Easings.CubicEaseInOut(Opacity)));

            canvas.SaveLayer(opacityPaint);

            foreach (ProfileElement profileElement in Children)
                profileElement.Render(canvas, basePosition, editorFocus);

            canvas.Restore();

            foreach (ProfileScript profileScript in Scripts)
                profileScript.OnProfileRendered(canvas, canvas.LocalClipBounds);

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

    /// <summary>
    ///     Starts the fade out process.
    /// </summary>
    public void FadeOut()
    {
        if (Disposed)
            throw new ObjectDisposedException("Profile");

        ShouldBeEnabled = false;
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

        while (Scripts.Count > 0)
            RemoveScript(Scripts[0]);

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

        List<RenderProfileElement> renderElements = GetAllRenderElements();

        if (ProfileEntity.LastSelectedProfileElement != Guid.Empty)
            LastSelectedProfileElement = renderElements.FirstOrDefault(f => f.EntityId == ProfileEntity.LastSelectedProfileElement);
        else
            LastSelectedProfileElement = null;

        while (_scriptConfigurations.Any())
            RemoveScriptConfiguration(_scriptConfigurations[0]);
        foreach (ScriptConfiguration scriptConfiguration in ProfileEntity.ScriptConfigurations.Select(e => new ScriptConfiguration(e)))
            AddScriptConfiguration(scriptConfiguration);

        // Load node scripts last since they may rely on the profile structure being in place
        foreach (RenderProfileElement renderProfileElement in renderElements)
            renderProfileElement.LoadNodeScript();
    }

    /// <summary>
    ///     Removes a script configuration from the profile, if the configuration has an active script it is also removed.
    /// </summary>
    internal void RemoveScriptConfiguration(ScriptConfiguration scriptConfiguration)
    {
        if (!_scriptConfigurations.Contains(scriptConfiguration))
            return;

        Script? script = scriptConfiguration.Script;
        if (script != null)
            RemoveScript((ProfileScript) script);

        _scriptConfigurations.Remove(scriptConfiguration);
    }

    /// <summary>
    ///     Adds a script configuration to the profile but does not instantiate it's script.
    /// </summary>
    internal void AddScriptConfiguration(ScriptConfiguration scriptConfiguration)
    {
        if (!_scriptConfigurations.Contains(scriptConfiguration))
            _scriptConfigurations.Add(scriptConfiguration);
    }

    /// <summary>
    ///     Adds a script that has a script configuration belonging to this profile.
    /// </summary>
    internal void AddScript(ProfileScript script)
    {
        if (!_scriptConfigurations.Contains(script.ScriptConfiguration))
            throw new ArtemisCoreException("Cannot add a script to a profile whose script configuration doesn't belong to the same profile.");

        if (!_scripts.Contains(script))
            _scripts.Add(script);
    }

    /// <summary>
    ///     Removes a script from the profile and disposes it.
    /// </summary>
    internal void RemoveScript(ProfileScript script)
    {
        _scripts.Remove(script);
        script.Dispose();
    }

    internal override void Save()
    {
        if (Disposed)
            throw new ObjectDisposedException("Profile");

        ProfileEntity.Id = EntityId;
        ProfileEntity.Name = Configuration.Name;
        ProfileEntity.IsFreshImport = IsFreshImport;
        ProfileEntity.LastSelectedProfileElement = LastSelectedProfileElement?.EntityId ?? Guid.Empty;

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