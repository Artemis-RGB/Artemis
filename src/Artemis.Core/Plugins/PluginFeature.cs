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


        /// <summary>
        ///     Gets the plugin feature info related to this feature
        /// </summary>
        public PluginFeatureInfo Info { get; internal set; } = null!; // Will be set right after construction

        /// <summary>
        ///     Gets the plugin that provides this feature
        /// </summary>
        public Plugin Plugin { get; internal set; } = null!; // Will be set right after construction

        /// <summary>
        ///     Gets the profiler that can be used to take profiling measurements
        /// </summary>
        public Profiler Profiler { get; internal set; } = null!; // Will be set right after construction

        /// <summary>
        ///     Gets whether the plugin is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            internal set => SetAndNotify(ref _isEnabled, value);
        }

        internal int AutoEnableAttempts { get; set; }

        /// <summary>
        ///     Gets the identifier of this plugin feature
        /// </summary>
        public string Id => $"{GetType().FullName}-{Plugin.Guid.ToString().Substring(0, 8)}"; // Not as unique as a GUID but good enough and stays readable

        internal PluginFeatureEntity Entity { get; set; } = null!; // Will be set right after construction

        /// <summary>
        ///     Called when the feature is activated
        /// </summary>
        public abstract void Enable();

        /// <summary>
        ///     Called when the feature is deactivated or when Artemis shuts down
        /// </summary>
        public abstract void Disable();

        /// <summary>
        ///     Occurs when the feature is enabled
        /// </summary>
        public event EventHandler? Enabled;

        /// <summary>
        ///     Occurs when the feature is disabled
        /// </summary>
        public event EventHandler? Disabled;

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
                SetEnabled(false);
        }

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

        internal void StartUpdateMeasure()
        {
            Profiler.StartMeasurement("Update");
        }

        internal void StopUpdateMeasure()
        {
            Profiler.StopMeasurement("Update");
        }

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
                    // Even if disable failed, still leave it in a disabled state to avoid more issues
                    InternalDisable();
                    IsEnabled = false;

                    OnDisabled();
                    return;
                }

                try
                {
                    if (isAutoEnable)
                        AutoEnableAttempts++;

                    if (isAutoEnable && GetLockFileCreated())
                    {
                        // Don't wrap existing lock exceptions, simply rethrow them
                        if (Info.LoadException is ArtemisPluginLockException)
                            throw Info.LoadException;

                        throw new ArtemisPluginLockException(Info.LoadException);
                    }

                    CreateLockFile();
                    IsEnabled = true;

                    // Allow up to 15 seconds for plugins to activate.
                    // This means plugins that need more time should do their long running tasks in a background thread, which is intentional
                    // This would've been a perfect match for Thread.Abort but that didn't make it into .NET Core
                    Task enableTask = Task.Run(InternalEnable);
                    if (!enableTask.Wait(TimeSpan.FromSeconds(15)))
                        throw new ArtemisPluginException(Plugin, "Plugin load timeout");

                    Info.LoadException = null;
                    AutoEnableAttempts = 0;
                    OnEnabled();
                }
                // If enable failed, put it back in a disabled state
                catch (Exception e)
                {
                    IsEnabled = false;
                    Info.LoadException = e;
                    throw;
                }
                finally
                {
                    // Clean up the lock file unless the failure was due to the lock file
                    // After all, we failed but not miserably :)
                    if (Info.LoadException is not ArtemisPluginLockException)
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
            if (IsEnabled)
                Disable();
        }

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

        #region Timed updates

        /// <summary>
        ///     Registers a timed update that whenever the plugin is enabled calls the provided <paramref name="action" /> at the
        ///     provided
        ///     <paramref name="interval" />
        /// </summary>
        /// <param name="interval">The interval at which the update should occur</param>
        /// <param name="action">
        ///     The action to call every time the interval has passed. The delta time parameter represents the
        ///     time passed since the last update in seconds
        /// </param>
        /// <param name="name">An optional name used in exceptions and profiling</param>
        /// <returns>The resulting plugin update registration which can be used to stop the update</returns>
        public TimedUpdateRegistration AddTimedUpdate(TimeSpan interval, Action<double> action, string? name = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return new TimedUpdateRegistration(this, interval, action, name);
        }

        /// <summary>
        ///     Registers a timed update that whenever the plugin is enabled calls the provided <paramref name="asyncAction" /> at the
        ///     provided
        ///     <paramref name="interval" />
        /// </summary>
        /// <param name="interval">The interval at which the update should occur</param>
        /// <param name="asyncAction">
        ///     The async action to call every time the interval has passed. The delta time parameter
        ///     represents the time passed since the last update in seconds
        /// </param>
        /// <param name="name">An optional name used in exceptions and profiling</param>
        /// <returns>The resulting plugin update registration</returns>
        public TimedUpdateRegistration AddTimedUpdate(TimeSpan interval, Func<double, Task> asyncAction, string? name = null)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));
            return new TimedUpdateRegistration(this, interval, asyncAction, name);
        }

        #endregion
    }
}