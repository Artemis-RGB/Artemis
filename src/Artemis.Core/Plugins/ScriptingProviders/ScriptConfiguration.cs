using System;
using Artemis.Storage.Entities.General;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents the configuration of a script
    /// </summary>
    public class ScriptConfiguration : CorePropertyChanged, IStorageModel
    {
        private bool _hasChanges;
        private string _name;
        private string? _pendingScriptContent;
        private string? _scriptContent;
        private string _scriptingProviderId;

        /// <summary>
        ///     Creates a new instance of the <see cref="ScriptConfiguration" /> class
        /// </summary>
        public ScriptConfiguration(ScriptingProvider provider, string name)
        {
            _scriptingProviderId = provider.Id;
            _name = name;
            Entity = new ScriptConfigurationEntity();
        }

        internal ScriptConfiguration(ScriptConfigurationEntity entity)
        {
            _scriptingProviderId = null!;
            _name = null!;
            Entity = entity;

            Load();
        }

        /// <summary>
        ///     Gets or sets the ID of the scripting provider
        /// </summary>
        public string ScriptingProviderId
        {
            get => _scriptingProviderId;
            set => SetAndNotify(ref _scriptingProviderId, value);
        }

        /// <summary>
        ///     Gets or sets the name of the script
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     Gets or sets the script's content
        /// </summary>
        public string? ScriptContent
        {
            get => _scriptContent;
            private set
            {
                if (!SetAndNotify(ref _scriptContent, value)) return;
                OnScriptContentChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the pending changes to the script's content
        /// </summary>
        public string? PendingScriptContent
        {
            get => _pendingScriptContent;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                if (!SetAndNotify(ref _pendingScriptContent, value)) return;
                HasChanges = ScriptContent != PendingScriptContent;
            }
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this configuration has pending changes to it's
        ///     <see cref="ScriptContent" />
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges;
            set => SetAndNotify(ref _hasChanges, value);
        }

        /// <summary>
        ///     If active, gets the script
        /// </summary>
        public Script? Script { get; internal set; }

        internal ScriptConfigurationEntity Entity { get; }

        /// <summary>
        ///     Applies the <see cref="PendingScriptContent" /> to the <see cref="ScriptContent" />
        /// </summary>
        public void ApplyPendingChanges()
        {
            ScriptContent = PendingScriptContent;
            HasChanges = false;
        }

        /// <summary>
        ///     Discards the <see cref="PendingScriptContent" />
        /// </summary>
        public void DiscardPendingChanges()
        {
            PendingScriptContent = ScriptContent;
            HasChanges = false;
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            ScriptingProviderId = Entity.ScriptingProviderId;
            ScriptContent = Entity.ScriptContent;
            PendingScriptContent = Entity.ScriptContent;
            Name = Entity.Name;
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.ScriptingProviderId = ScriptingProviderId;
            Entity.ScriptContent = ScriptContent;
            Entity.Name = Name;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs whenever the contents of the script have changed
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