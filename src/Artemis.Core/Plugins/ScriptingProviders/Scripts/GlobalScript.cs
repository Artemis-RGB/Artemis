namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script running globally
    /// </summary>
    public abstract class GlobalScript : Script
    {
        /// <summary>
        ///     Called whenever the Artemis Core is about to update
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnCoreUpdating(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the Artemis Core has been updated
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnCoreUpdated(double deltaTime)
        {
        }
    }
}