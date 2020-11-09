using System;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an implementation of a certain type provided by a plugin
    /// </summary>
    public abstract class PluginImplementation : IDisposable
    {
        /// <summary>
        ///     Gets the plugin that provides this implementation
        /// </summary>
        public Plugin Plugin { get; internal set; }

        /// <summary>
        ///     Gets the plugin info related to this plugin
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets whether the plugin is enabled
        /// </summary>
        public bool IsEnabled { get; internal set; }

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
            if (enable && !IsEnabled)
            {
                try
                {
                    if (isAutoEnable && PluginInfo.GetLockFileCreated())
                    {
                        // Don't wrap existing lock exceptions, simply rethrow them
                        if (PluginInfo.LoadException is ArtemisPluginLockException)
                            throw PluginInfo.LoadException;

                        throw new ArtemisPluginLockException(PluginInfo.LoadException);
                    }

                    IsEnabled = true;
                    PluginInfo.IsEnabled = true;
                    PluginInfo.CreateLockFile();

                    // Allow up to 15 seconds for plugins to activate.
                    // This means plugins that need more time should do their long running tasks in a background thread, which is intentional
                    // Little meh: Running this from a different thread could cause deadlocks
                    Task enableTask = Task.Run(InternalEnable);
                    if (!enableTask.Wait(TimeSpan.FromSeconds(15)))
                        throw new ArtemisPluginException(PluginInfo, "Plugin load timeout");

                    PluginInfo.LoadException = null;
                    OnEnabled();
                }
                // If enable failed, put it back in a disabled state
                catch (Exception e)
                {
                    IsEnabled = false;
                    PluginInfo.IsEnabled = false;
                    PluginInfo.LoadException = e;
                    throw;
                }
                finally
                {
                    if (!(PluginInfo.LoadException is ArtemisPluginLockException))
                        PluginInfo.DeleteLockFile();
                }
            }
            else if (!enable && IsEnabled)
            {
                IsEnabled = false;
                PluginInfo.IsEnabled = false;

                // Even if disable failed, still leave it in a disabled state to avoid more issues
                InternalDisable();
                OnDisabled();
            }
            // A failed load is still enabled in plugin info (to avoid disabling it permanently after a fail)
            // update even that when manually disabling
            else if (!enable && !IsEnabled)
            {
                PluginInfo.IsEnabled = false;
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