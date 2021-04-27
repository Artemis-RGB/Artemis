using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Artemis.Storage.Entities.Plugins;
using McMaster.NETCore.Plugins;
using Ninject;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin
    /// </summary>
    public class Plugin : CorePropertyChanged, IDisposable
    {
        private readonly List<PluginFeatureInfo> _features;

        private bool _isEnabled;

        internal Plugin(PluginInfo info, DirectoryInfo directory, PluginEntity? pluginEntity)
        {
            Info = info;
            Directory = directory;
            Entity = pluginEntity ?? new PluginEntity {Id = Guid, IsEnabled = true};

            _features = new List<PluginFeatureInfo>();
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
        public ReadOnlyCollection<PluginFeatureInfo> Features => _features.AsReadOnly();

        /// <summary>
        ///     The assembly the plugin code lives in
        /// </summary>
        public Assembly? Assembly { get; internal set; }

        /// <summary>
        ///     Gets the plugin bootstrapper
        /// </summary>
        public IPluginBootstrapper? Bootstrapper { get; internal set; }

        /// <summary>
        ///     The Ninject kernel of the plugin
        /// </summary>
        public IKernel? Kernel { get; internal set; }

        /// <summary>
        ///     Gets a list of prerequisites for this plugin feature
        /// </summary>
        public List<PluginPrerequisite> Prerequisites { get; } = new();

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

        /// <inheritdoc />
        public override string ToString()
        {
            return Info.ToString();
        }

        /// <summary>
        ///     Determines whether the prerequisites of this plugin are met
        /// </summary>
        public bool ArePrerequisitesMet()
        {
            return Prerequisites.All(p => p.IsMet());
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

                Kernel?.Dispose();
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

        internal void ApplyToEntity()
        {
            Entity.Id = Guid;
            Entity.IsEnabled = IsEnabled;
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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}