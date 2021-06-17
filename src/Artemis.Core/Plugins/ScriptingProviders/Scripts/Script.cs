using System;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script processed by a <see cref="ScriptingProviders.ScriptingProvider" />.
    /// </summary>
    public abstract class Script : CorePropertyChanged, IDisposable
    {
        private bool _isAvailable;
        private string? _scriptContent;
        private ScriptingProvider? _scriptingProvider;

        /// <summary>
        ///     Gets the scripting provider this script belongs to
        /// </summary>
        public ScriptingProvider? ScriptingProvider
        {
            get => _scriptingProvider;
            internal set => SetAndNotify(ref _scriptingProvider, value);
        }

        /// <summary>
        ///     Gets a boolean indicating whether this script is available
        /// </summary>
        public bool IsAvailable
        {
            get => _isAvailable;
            private set => SetAndNotify(ref _isAvailable, value);
        }

        /// <summary>
        ///     Gets or sets the content of the script
        /// </summary>
        public string? ScriptContent
        {
            get => _scriptContent;
            set
            {
                if (!SetAndNotify(ref _scriptContent, value)) return;
                OnScriptContentChanged();
            }
        }

        internal void Initialize(ScriptingProvider scriptingProvider)
        {
            if (ScriptingProvider != null)
                throw new ArtemisCoreException("Script is already initialized");

            ScriptingProvider = scriptingProvider;
            ScriptingProvider.Disabled += ScriptingProviderOnDisabled;
            IsAvailable = true;
        }


        private void ScriptingProviderOnDisabled(object? sender, EventArgs e)
        {
            IsAvailable = false;
            Dispose(true);
            ScriptingProvider = null;
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