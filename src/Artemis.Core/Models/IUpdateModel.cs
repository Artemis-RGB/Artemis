namespace Artemis.Core
{
    /// <summary>
    /// Represents a model that updates using a delta time
    /// </summary>
    public interface IUpdateModel
    {
        /// <summary>
        ///     Performs an update on the model
        /// </summary>
        /// <param name="deltaTime">The delta time in seconds</param>
        void Update(double deltaTime);
    }
}