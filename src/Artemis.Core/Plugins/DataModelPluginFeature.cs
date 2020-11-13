using System;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an feature of a certain type provided by a plugin with support for data models
    /// </summary>
    public abstract class DataModelPluginFeature : PluginFeature
    {
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
        /// <returns>The resulting plugin update registration which can be used to stop the update</returns>
        public TimedUpdateRegistration AddTimedUpdate(TimeSpan interval, Action<double> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            return new TimedUpdateRegistration(this, interval, action);
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
        /// <returns>The resulting plugin update registration</returns>
        public TimedUpdateRegistration AddTimedUpdate(TimeSpan interval, Func<double, Task> asyncAction)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));
            return new TimedUpdateRegistration(this, interval, asyncAction);
        }
    }
}