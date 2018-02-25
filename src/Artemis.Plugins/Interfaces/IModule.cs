namespace Artemis.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public interface IModule : IPlugin
    {
        /// <summary>
        ///     Called each frame when the module must update
        /// </summary>
        /// <param name="deltaTime">Time since the last update</param>
        void Update(double deltaTime);

        /// <summary>
        ///     Called each frame when the module must render
        /// </summary>
        /// <param name="deltaTime">Time since the last render</param>
        void Render(double deltaTime);
    }
}