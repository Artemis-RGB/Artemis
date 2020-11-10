using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Storage.Entities.Plugins;
using Stylet;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an implementation of a certain type provided by a plugin
    /// </summary>
    public abstract class PluginImplementation : PropertyChangedBase, IDisposable
    {
        private Exception? _loadException;
        private bool _isEnabled;

        /// <summary>
        ///     Gets the plugin that provides this implementation
        /// </summary>
        public Plugin? Plugin { get; internal set; }

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

        internal PluginImplementationEntity? Entity { get; set; }

        /// <summary>
        ///     Called when the implementation is activated
        /// </summary>
        public abstract void Enable();

        /// <summary>
        ///     Called when the implementation is deactivated or when Artemis shuts down
        /// </summary>
        public abstract void Disable();

        internal void SetEnabled(bool enable, bool isAutoEnable = false)
        {
            if (enable == IsEnabled)
                return;

            if (Plugin == null)
                throw new ArtemisCoreException("Cannot enable a plugin implementation that is not associated with a plugin");

            lock (Plugin)
            {
                if (!Plugin.IsEnabled)
                    throw new ArtemisCoreException("Cannot enable a plugin implementation of a disabled plugin");

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

                    IsEnabled = true;
                    CreateLockFile();

                    // Allow up to 15 seconds for plugins to activate.
                    // This means plugins that need more time should do their long running tasks in a background thread, which is intentional
                    ManualResetEvent wait = new ManualResetEvent(false);
                    Thread work = new Thread(() =>
                    {
                        InternalEnable();
                        wait.Set();
                    });
                    work.Start();
                    wait.WaitOne(TimeSpan.FromSeconds(15));
                    if (work.IsAlive)
                    {
                        work.Abort();
                        throw new ArtemisPluginException(Plugin, "Plugin load timeout");
                    }

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

        /// <inheritdoc />
        public void Dispose()
        {
            Disable();
        }

        #region Loading

        internal void CreateLockFile()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin implementation that is not associated with a plugin");

            File.Create(Plugin.ResolveRelativePath($"{GetType().FullName}.lock")).Close();
        }

        internal void DeleteLockFile()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin implementation that is not associated with a plugin");

            if (GetLockFileCreated())
                File.Delete(Plugin.ResolveRelativePath($"{GetType().FullName}.lock"));
        }

        internal bool GetLockFileCreated()
        {
            if (Plugin == null)
                throw new ArtemisCoreException("Cannot lock a plugin implementation that is not associated with a plugin");

            return File.Exists(Plugin.ResolveRelativePath($"{GetType().FullName}.lock"));
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the implementation is enabled
        /// </summary>
        public event EventHandler? Enabled;

        /// <summary>
        ///     Occurs when the implementation is disabled
        /// </summary>
        public event EventHandler? Disabled;

        /// <summary>
        ///     Triggers the PluginEnabled event
        /// </summary>
        protected virtual void OnEnabled()
        {
            Enabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Triggers the PluginDisabled event
        /// </summary>
        protected virtual void OnDisabled()
        {
            Disabled?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}