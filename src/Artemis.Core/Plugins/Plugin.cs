using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Artemis.Core.DeviceProviders;
using Artemis.Storage.Entities.Plugins;
using DryIoc;
using McMaster.NETCore.Plugins;

namespace Artemis.Core;

/// <summary>
///     Represents a plugin
/// </summary>
public class Plugin : CorePropertyChanged, IDisposable
{
    private readonly List<PluginFeatureInfo> _features;
    private readonly bool _loadedFromStorage;
    private readonly List<Profiler> _profilers;

    private bool _isEnabled;

    internal Plugin(PluginInfo info, DirectoryInfo directory, PluginEntity pluginEntity, bool loadedFromStorage)
    {
        Info = info;
        Directory = directory;
        Entity = pluginEntity;
        Info.Plugin = this;

        _loadedFromStorage = loadedFromStorage;
        _features = new List<PluginFeatureInfo>();
        _profilers = new List<Profiler>();

        Features = new ReadOnlyCollection<PluginFeatureInfo>(_features);
        Profilers = new ReadOnlyCollection<Profiler>(_profilers);
    }

    /// <summary>
    ///     Gets the plugin GUID
    /// </summary>
    public Guid Guid => Info.Guid;

    /// <summary>
    ///     Gets the plugin info related to this plugin
    /// </summary>
    public PluginInfo Info { get; }

    /// <summary>
    ///     The plugins root directory
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    ///     Gets or sets a configuration dialog for this plugin that is accessible in the UI under Settings > Plugins
    /// </summary>
    public IPluginConfigurationDialog? ConfigurationDialog { get; set; }
    
    /// <summary>
    ///     Indicates whether the user enabled the plugin or not
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        private set => SetAndNotify(ref _isEnabled, value);
    }

    /// <summary>
    ///     Gets a read-only collection of all features this plugin provides
    /// </summary>
    public ReadOnlyCollection<PluginFeatureInfo> Features { get; }

    /// <summary>
    ///     Gets a read-only collection of profiles running on the plugin
    /// </summary>
    public ReadOnlyCollection<Profiler> Profilers { get; }

    /// <summary>
    ///     The assembly the plugin code lives in
    /// </summary>
    public Assembly? Assembly { get; internal set; }

    /// <summary>
    ///     Gets the plugin bootstrapper
    /// </summary>
    public PluginBootstrapper? Bootstrapper { get; internal set; }

    /// <summary>
    ///     Gets the IOC container of the plugin, only use this for advanced IOC operations, otherwise see <see cref="Resolve"/> and <see cref="Resolve{T}"/>
    /// </summary>
    public IContainer? Container { get; internal set; }

    /// <summary>
    ///     The PluginLoader backing this plugin
    /// </summary>
    internal PluginLoader? PluginLoader { get; set; }

    /// <summary>
    ///     The entity representing the plugin
    /// </summary>
    internal PluginEntity Entity { get; set; }

    /// <summary>
    ///     Populated when plugin settings are first loaded
    /// </summary>
    internal PluginSettings? Settings { get; set; }

    /// <summary>
    ///     Resolves the relative path provided in the <paramref name="path" /> parameter to an absolute path
    /// </summary>
    /// <param name="path">The path to resolve</param>
    /// <returns>An absolute path pointing to the provided relative path</returns>
    [return: NotNullIfNotNull("path")]
    public string? ResolveRelativePath(string? path)
    {
        return path == null ? null : Path.Combine(Directory.FullName, path);
    }

    /// <summary>
    ///     Looks up the instance of the feature of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of feature to find</typeparam>
    /// <returns>If found, the instance of the feature</returns>
    public T? GetFeature<T>() where T : PluginFeature
    {
        return _features.FirstOrDefault(i => i.Instance is T)?.Instance as T;
    }

    /// <summary>
    ///     Looks up the feature info the feature of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of feature to find</typeparam>
    /// <returns>Feature info of the feature</returns>
    public PluginFeatureInfo GetFeatureInfo<T>() where T : PluginFeature
    {
        // This should be a safe assumption because any type of PluginFeature is registered and added
        return _features.First(i => i.FeatureType == typeof(T));
    }

    /// <summary>
    ///     Gets a profiler with the provided <paramref name="name" />, if it does not yet exist it will be created.
    /// </summary>
    /// <param name="name">The name of the profiler</param>
    /// <returns>A new or existing profiler with the provided <paramref name="name" /></returns>
    public Profiler GetProfiler(string name)
    {
        Profiler? profiler = _profilers.FirstOrDefault(p => p.Name == name);
        if (profiler != null)
            return profiler;

        profiler = new Profiler(this, name);
        _profilers.Add(profiler);
        return profiler;
    }

    /// <summary>
    ///     Removes a profiler from the plugin
    /// </summary>
    /// <param name="profiler">The profiler to remove</param>
    public void RemoveProfiler(Profiler profiler)
    {
        _profilers.Remove(profiler);
    }

    /// <summary>
    ///     Gets an instance of the specified service using the plugins dependency injection container.    
    /// </summary>
    /// <param name="arguments">Arguments to supply to the service.</param>
    /// <typeparam name="T">The service to resolve.</typeparam>
    /// <returns>An instance of the service.</returns>
    /// <seealso cref="Resolve"/>
    public T Resolve<T>(params object?[] arguments)
    {
        if (Container == null)
            throw new ArtemisPluginException("Cannot use Resolve<T> before the plugin finished loading");
        return Container.Resolve<T>(args: arguments);
    }

    /// <summary>
    ///     Gets an instance of the specified service using the plugins dependency injection container.     
    /// </summary>
    /// <param name="type">The type of service to resolve.</param>
    /// <param name="arguments">Arguments to supply to the service.</param>
    /// <returns>An instance of the service.</returns>
    /// <seealso cref="Resolve{T}"/>
    public object Resolve(Type type, params object?[] arguments)
    {
        if (Container == null)
            throw new ArtemisPluginException("Cannot use Resolve before the plugin finished loading");
        return Container.Resolve(type, args: arguments);
    }

    /// <summary>
    ///     Registers service of <typeparamref name="TService" /> type implemented by <typeparamref name="TImplementation" /> type.
    /// </summary>
    /// <param name="scope">The scope in which the service should live, if you are not sure leave it on singleton.</param>
    /// <typeparam name="TService">The service to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation of the service to register.</typeparam>
    public void Register<TService, TImplementation>(PluginServiceScope scope = PluginServiceScope.Singleton) where TImplementation : TService
    {
        IReuse reuse = scope switch
        {
            PluginServiceScope.Transient => Reuse.Transient,
            PluginServiceScope.Singleton => Reuse.Singleton,
            PluginServiceScope.Scoped => Reuse.Scoped,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };
        Container.Register<TService, TImplementation>(reuse);
    }

    /// <summary>
    ///     Registers implementation type <typeparamref name="TImplementation" /> with itself as service type.
    /// </summary>
    /// <param name="scope">The scope in which the service should live, if you are not sure leave it on singleton.</param>
    /// <typeparam name="TImplementation">The implementation of the service to register.</typeparam>
    public void Register<TImplementation>(PluginServiceScope scope = PluginServiceScope.Singleton)
    {
        IReuse reuse = scope switch
        {
            PluginServiceScope.Transient => Reuse.Transient,
            PluginServiceScope.Singleton => Reuse.Singleton,
            PluginServiceScope.Scoped => Reuse.Scoped,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };
        Container.Register<TImplementation>(reuse);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Info.ToString();
    }

    /// <summary>
    ///     Occurs when the plugin is enabled
    /// </summary>
    public event EventHandler? Enabled;

    /// <summary>
    ///     Occurs when the plugin is disabled
    /// </summary>
    public event EventHandler? Disabled;

    /// <summary>
    ///     Occurs when an feature is loaded and added to the plugin
    /// </summary>
    public event EventHandler<PluginFeatureInfoEventArgs>? FeatureAdded;

    /// <summary>
    ///     Occurs when an feature is disabled and removed from the plugin
    /// </summary>
    public event EventHandler<PluginFeatureInfoEventArgs>? FeatureRemoved;

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (PluginFeatureInfo feature in Features)
                feature.Instance?.Dispose();
            SetEnabled(false);

            Container?.Dispose();
            PluginLoader?.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _features.Clear();
        }
    }

    /// <summary>
    ///     Invokes the Enabled event
    /// </summary>
    protected virtual void OnEnabled()
    {
        Enabled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invokes the Disabled event
    /// </summary>
    protected virtual void OnDisabled()
    {
        Disabled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invokes the FeatureAdded event
    /// </summary>
    protected virtual void OnFeatureAdded(PluginFeatureInfoEventArgs e)
    {
        FeatureAdded?.Invoke(this, e);
    }

    /// <summary>
    ///     Invokes the FeatureRemoved event
    /// </summary>
    protected virtual void OnFeatureRemoved(PluginFeatureInfoEventArgs e)
    {
        FeatureRemoved?.Invoke(this, e);
    }
    
    internal void AddFeature(PluginFeatureInfo featureInfo)
    {
        if (featureInfo.Plugin != this)
            throw new ArtemisCoreException("Feature is not associated with this plugin");
        _features.Add(featureInfo);

        OnFeatureAdded(new PluginFeatureInfoEventArgs(featureInfo));
    }

    internal void RemoveFeature(PluginFeatureInfo featureInfo)
    {
        if (featureInfo.Instance != null && featureInfo.Instance.IsEnabled)
            throw new ArtemisCoreException("Cannot remove an enabled feature from a plugin");

        _features.Remove(featureInfo);
        featureInfo.Instance?.Dispose();

        OnFeatureRemoved(new PluginFeatureInfoEventArgs(featureInfo));
    }

    internal void SetEnabled(bool enable)
    {
        if (IsEnabled == enable)
            return;

        if (!enable && Features.Any(e => e.Instance != null && e.Instance.IsEnabled))
            throw new ArtemisCoreException("Cannot disable this plugin because it still has enabled features");

        IsEnabled = enable;

        if (enable)
        {
            Bootstrapper?.OnPluginEnabled(this);
            OnEnabled();
        }
        else
        {
            Bootstrapper?.OnPluginDisabled(this);
            OnDisabled();
        }
    }

    internal bool HasEnabledFeatures()
    {
        return Entity.Features.Any(f => f.IsEnabled) || Features.Any(f => f.AlwaysEnabled);
    }

    internal void AutoEnableIfNew()
    {
        if (_loadedFromStorage)
            return;

        // Enabled is preset to true if the plugin meets the following criteria
        // - Requires no admin rights
        // - No always-enabled device providers
        // - Either has no prerequisites or they are all met
        Entity.IsEnabled = !Info.RequiresAdmin &&
                           Features.All(f => !f.AlwaysEnabled || !f.FeatureType.IsAssignableTo(typeof(DeviceProvider))) &&
                           Info.ArePrerequisitesMet();

        if (!Entity.IsEnabled)
            return;

        // Also auto-enable any non-device provider feature
        foreach (PluginFeatureInfo pluginFeatureInfo in Features)
            pluginFeatureInfo.Entity.IsEnabled = !pluginFeatureInfo.FeatureType.IsAssignableTo(typeof(DeviceProvider));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
///     Represents a scope in which a plugin service is injected by the IOC container.
/// </summary>
public enum PluginServiceScope
{
    /// <summary>
    ///     Services in this scope are never reused, a new instance is injected each time.
    /// </summary>
    Transient,

    /// <summary>
    ///     Services in this scope are reused for as long as the plugin lives, the same instance is injected each time.
    /// </summary>
    Singleton,

    /// <summary>
    ///     Services in this scope are reused within a container scope, this is an advanced setting you shouldn't need.
    ///     <para>To learn more see <a href="https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/ReuseAndScopes.md#reusescoped">the DryIoc docs</a>.</para>
    /// </summary>
    Scoped
}