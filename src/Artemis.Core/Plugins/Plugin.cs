using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly List<PluginFeature> _features;

        private bool _isEnabled;

        internal Plugin(PluginInfo info, DirectoryInfo directory)
        {
            Info = info;
            Directory = directory;

            _features = new List<PluginFeature>();
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
        public ReadOnlyCollection<PluginFeature> Features => _features.AsReadOnly();

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
        ///     The PluginLoader backing this plugin
        /// </summary>
        internal PluginLoader? PluginLoader { get; set; }

        /// <summary>
        ///     The entity representing the plugin
        /// </summary>
        internal PluginEntity Entity { get; set; }

        /// <summary>
        ///     Resolves the relative path provided in the <paramref name="path" /> parameter to an absolute path
        /// </summary>
        /// <param name="path">The path to resolve</param>
        /// <returns>An absolute path pointing to the provided relative path</returns>
        public string? ResolveRelativePath(string path)
        {
            return path == null ? null : Path.Combine(Directory.FullName, path);
        }

        /// <summary>
        ///     Looks up the instance of the feature of type <typeparamref name="T" />
        ///     <para>Note: This method only returns instances of enabled features</para>
        /// </summary>
        /// <typeparam name="T">The type of feature to find</typeparam>
        /// <returns>If found, the instance of the feature</returns>
        public T? GetFeature<T>() where T : PluginFeature
        {
            return _features.FirstOrDefault(i => i is T) as T;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Info.ToString();
        }

        internal void ApplyToEntity()
        {
            Entity.Id = Guid;
            Entity.IsEnabled = IsEnabled;
        }

        internal void AddFeature(PluginFeature feature)
        {
            feature.Plugin = this;
            _features.Add(feature);

            OnFeatureAdded(new PluginFeatureEventArgs(feature));
        }

        internal void RemoveFeature(PluginFeature feature)
        {
            if (feature.IsEnabled)
                throw new ArtemisCoreException("Cannot remove an enabled feature from a plugin");
            
            _features.Remove(feature);
            feature.Dispose();

            OnFeatureRemoved(new PluginFeatureEventArgs(feature));
        }
        
        internal void SetEnabled(bool enable)
        {
            if (IsEnabled == enable)
                return;

            if (!enable && Features.Any(e => e.IsEnabled))
                throw new ArtemisCoreException("Cannot disable this plugin because it still has enabled features");

            IsEnabled = enable;

            if (enable)
            {
                Bootstrapper?.Enable(this);
                OnEnabled();
            }
            else
            {
                Bootstrapper?.Disable(this);
                OnDisabled();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (PluginFeature feature in Features)
                feature.Dispose();

            Kernel?.Dispose();
            PluginLoader?.Dispose();

            _features.Clear();
            SetEnabled(false);
        }

        #region Events

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
        public event EventHandler<PluginFeatureEventArgs>? FeatureAdded;

        /// <summary>
        ///     Occurs when an feature is disabled and removed from the plugin
        /// </summary>
        public event EventHandler<PluginFeatureEventArgs>? FeatureRemoved;

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
        protected virtual void OnFeatureAdded(PluginFeatureEventArgs e)
        {
            FeatureAdded?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the FeatureRemoved event
        /// </summary>
        protected virtual void OnFeatureRemoved(PluginFeatureEventArgs e)
        {
            FeatureRemoved?.Invoke(this, e);
        }

        #endregion
    }
}