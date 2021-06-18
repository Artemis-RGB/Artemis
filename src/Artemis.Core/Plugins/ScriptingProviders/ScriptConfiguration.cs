using Artemis.Storage.Entities.General;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents the configuration of a script
    /// </summary>
    public class ScriptConfiguration : CorePropertyChanged, IStorageModel
    {
        private string _scriptingProviderId;
        private string _name;
        private string? _scriptContent;

        /// <summary>
        ///     Creates a new instance of the <see cref="ScriptConfiguration" /> class
        /// </summary>
        public ScriptConfiguration(ScriptingProvider provider, string name)
        {
            ScriptingProviderId = provider.Id;
            Name = name;
            Entity = new ScriptConfigurationEntity();
        }

        internal ScriptConfiguration(ScriptConfigurationEntity entity)
        {
            ScriptingProviderId = null!;
            Name = null!;
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
            set => SetAndNotify(ref _scriptContent, value);
        }

        /// <summary>
        ///     If active, gets the script
        /// </summary>
        public Script? Script { get; internal set; }

        internal ScriptConfigurationEntity Entity { get; }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            ScriptingProviderId = Entity.ScriptingProviderId;
            ScriptContent = Entity.ScriptContent;
            Name = Entity.Name;
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.ScriptingProviderId = ScriptingProviderId;
            Entity.ScriptContent = ScriptContent;
        }

        #endregion
    }
}