using System;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script processed by a <see cref="ScriptingProviders.ScriptingProvider" />.
    /// </summary>
    public abstract class Script : IDisposable
    {
        /// <summary>
        ///     Gets the scripting provider this script belongs to
        /// </summary>
        public ScriptingProvider ScriptingProvider { get; internal set; } = null!;

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

        #region IDisposable

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
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}