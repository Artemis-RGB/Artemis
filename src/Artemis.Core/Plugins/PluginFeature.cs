using System;
using System.IO;
using System.Threading.Tasks;
using Artemis.Storage.Entities.Plugins;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an feature of a certain type provided by a plugin
    /// </summary>
    public abstract class PluginFeature : CorePropertyChanged, IDisposable
    {
        private bool _isEnabled;
        private Exception? _loadException;

        /// <summary>
        ///     Gets the plugin that provides this feature
        /// </summary>
        public Plugin Plugin { get; internal set; }

        /// <summary>
        ///     Gets whether the plugin is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            internal set => SetAndNotify(ref _isEnabled, value);
        }

        /// <summary>
        ///     Gets the exception thrown while loading
        /// </summary>
        public Exception? LoadException
        {
            get => _loadException;
            internal set => SetAndNotify(ref _loadException, value);
        }

        /// <summary>
        ///     Gets the identifier of this plugin feature
        /// </summary>
        public string Id => $"{GetType().FullName}-{Plugin.Guid.ToString().Substring(0, 8)}"; // Not as unique as a GUID but good enough and stays readable

        internal PluginFeatureEntity Entity { get; set; }

        /// <summary>
        ///     Called when the feature is activated
        /// </summary>
        public abstract void Enable();

        /// <summary>
        ///     Called when the feature is deactivated or when Artemis shuts down
        /// </summary>
        public abstract void Disable();

        internal void SetEnabled(bool enable, bool isAutoEnable = false)
        {
            if (enable == IsEnabled)
                return;

            if (Plugin == null)
                throw new ArtemisCoreException("Cannot enable a plugin feature that is not associated with a plugin");

            lock (Plugin)
            {
                if (!Plugin.IsEnabled)
                    throw new ArtemisCoreException("Cannot enable a plugin feature of a disabled plugin");

                if (!enable)
                {
                    IsEnabled = false;

                    // Even if disable failed, still leave it in a disabled state to avoid more issues
                    InternalDisable();
                    OnDisabled();
                    return;
                }

                try
                {
                    if (isAutoEnable && GetLockFileCreated())
                    {
                        // Don't wrap existing lock exceptions, simply rethrow them
                        if (LoadException is ArtemisPluginLockException)
                            throw LoadException;

                        throw new ArtemisPluginLockException(LoadException);
                    }

                    CreateLockFile();
                    IsEnabled = true;

                    // Allow up to 15 seconds for plugins to activate.
                    // This means plugins that need more time should do their long running tasks in a background thread, which is intentional
                    // This would've been a perfect match for Thread.Abort but that didn't make it into .NET Core
                    Task enableTask = Task.Run(InternalEnable);
                    if (!enableTask.Wait(TimeSpan.FromSeconds(15)))
                        throw new ArtemisPluginException(Plugin, "Plugin load timeout");

                    LoadException = null;
                    OnEnabled();
                }
                // If enable failed, put it back in a disabled state
                catch (Exception e)
                {
                    IsEnabled = false;
                    LoadException = e;
                    throw;
                }
                finally
                {
                    // Clean up the lock file unless the failure was due to the lock file
                    // After all, we failed but not miserably :)
                    if (!(LoadException is ArtemisPluginLockException))
                        DeleteLockFile();
                }
            }
        }

        internal virtual void InternalEnable()
        {
            Enable();
        }

        internal virtual void InternalDisable()
        {
            Disable();
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the plugin feature and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disable();
            }
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Loading

        internal void CreateLockFile()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin feature that is not associated with a plugin");

            File.Create(Plugin.ResolveRelativePath($"{GetType().FullName}.lock")).Close();
        }

        internal void DeleteLockFile()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin feature that is not associated with a plugin");

            if (GetLockFileCreated())
                File.Delete(Plugin.ResolveRelativePath($"{GetType().FullName}.lock"));
        }

        internal bool GetLockFileCreated()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin feature that is not associated with a plugin");

            return File.Exists(Plugin.ResolveRelativePath($"{GetType().FullName}.lock"));
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the feature is enabled
        /// </summary>
        public event EventHandler? Enabled;

        /// <summary>
        ///     Occurs when the feature is disabled
        /// </summary>
        public event EventHandler? Disabled;

        /// <summary>
        ///     Triggers the Enabled event
        /// </summary>
        protected virtual void OnEnabled()
        {
            Enabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Triggers the Disabled event
        /// </summary>
        protected virtual void OnDisabled()
        {
            Disabled?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}