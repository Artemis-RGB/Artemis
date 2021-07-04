using Artemis.Core.Services;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script running globally
    /// </summary>
    public abstract class GlobalScript : Script
    {
        /// <inheritdoc />
        protected GlobalScript(ScriptConfiguration configuration) : base(configuration)
        {
        }

        internal ScriptingService? ScriptingService { get; set; }

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

        #region Overrides of Script

        /// <inheritdoc />
        public override ScriptType ScriptType => ScriptType.Global;

        /// <inheritdoc />
        internal override void InternalCleanup()
        {
            ScriptingService?.InternalGlobalScripts.Remove(this);
        }

        #endregion
    }
}