namespace Artemis.Core
{
    /// <summary>
    /// Represents a model that can be loaded and saved to persistent storage
    /// </summary>
    public interface IStorageModel
    {
        /// <summary>
        ///     Loads the model from its associated entity
        /// </summary>
        void Load();

        /// <summary>
        ///     Saves the model to its associated entity
        /// </summary>
        void Save();
    }
}