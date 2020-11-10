using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Artemis.Storage.Entities.Plugins;
using McMaster.NETCore.Plugins;
using Ninject;
using Stylet;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin
    /// </summary>
    public class Plugin : PropertyChangedBase, IDisposable
    {
        private readonly List<PluginImplementation> _implementations;

        private bool _isEnabled;

        internal Plugin(PluginInfo info, DirectoryInfo directory)
        {
            Info = info;
            Directory = directory;

            _implementations = new List<PluginImplementation>();
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
        public PluginConfigurationDialog? ConfigurationDialog { get; protected set; }
        
        /// <summary>
        ///     Indicates whether the user enabled the plugin or not
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            private set => SetAndNotify(ref _isEnabled, value);
        }

        /// <summary>
        ///     Gets a read-only collection of all implementations this plugin provides
        /// </summary>
        public ReadOnlyCollection<PluginImplementation> Implementations => _implementations.AsReadOnly();

        /// <summary>
        ///     The assembly the plugin code lives in
        /// </summary>
        internal Assembly? Assembly { get; set; }

        /// <summary>
        ///     The Ninject kernel of the plugin
        /// </summary>
        internal IKernel? Kernel { get; set; }

        /// <summary>
        ///     The PluginLoader backing this plugin
        /// </summary>
        internal PluginLoader? PluginLoader { get; set; }

        /// <summary>
        ///     The entity representing the plugin
        /// </summary>
        internal PluginEntity? Entity { get; set; }

        /// <summary>
        ///     Resolves the relative path provided in the <paramref name="path" /> parameter to an absolute path
        /// </summary>
        /// <param name="path">The path to resolve</param>
        /// <returns>An absolute path pointing to the provided relative path</returns>
        public string? ResolveRelativePath(string path)
        {
            return path == null ? null : Path.Combine(Directory.FullName, path);
        }

        internal void ApplyToEntity()
        {
            Entity.Id = Guid;
            Entity.IsEnabled = IsEnabled;
        }

        internal void AddImplementation(PluginImplementation implementation)
        {
            implementation.Plugin = this;
            _implementations.Add(implementation);
        }

        public void SetEnabled(bool enable)
        {
            if (IsEnabled == enable)
                return;

            if (!enable && Implementations.Any(e => e.IsEnabled))
                throw new ArtemisCoreException("Cannot disable this plugin because it still has enabled implementations");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Info.ToString();
        }

        public void Dispose()
        {
            foreach (PluginImplementation pluginImplementation in Implementations) 
                pluginImplementation.Dispose();

            Kernel?.Dispose();
            PluginLoader?.Dispose();
        }
    }
}