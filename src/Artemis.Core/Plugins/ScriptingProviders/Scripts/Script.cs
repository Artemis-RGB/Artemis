using System;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script processed by a <see cref="ScriptingProviders.ScriptingProvider" />.
    /// </summary>
    public abstract class Script : CorePropertyChanged, IDisposable
    {
        private ScriptConfiguration _scriptConfiguration;
        private ScriptingProvider _scriptingProvider;
        
        /// <summary>
        ///     Gets the scripting provider this script belongs to
        /// </summary>
        public ScriptingProvider ScriptingProvider
        {
            get => _scriptingProvider;
            internal set => SetAndNotify(ref _scriptingProvider, value);
        }

        /// <summary>
        ///     Gets the script configuration this script belongs to
        /// </summary>
        public ScriptConfiguration ScriptConfiguration
        {
            get => _scriptConfiguration;
            internal set => SetAndNotify(ref _scriptConfiguration, value);
        }

        internal void Initialize(ScriptingProvider scriptingProvider)
        {
            if (ScriptingProvider != null)
                throw new ArtemisCoreException("Script is already initialized");

            ScriptingProvider = scriptingProvider;
            ScriptingProvider.Disabled += ScriptingProviderOnDisabled;
        }


        private void ScriptingProviderOnDisabled(object? sender, EventArgs e)
        {
            Dispose(true);
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


        #region Events

        /// <summary>
        ///     Occurs when the contents of the script have changed
        /// </summary>
        public event EventHandler? ScriptContentChanged;

        /// <summary>
        ///     Invokes the <see cref="ScriptContentChanged" /> event
        /// </summary>
        protected virtual void OnScriptContentChanged()
        {
            ScriptContentChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}