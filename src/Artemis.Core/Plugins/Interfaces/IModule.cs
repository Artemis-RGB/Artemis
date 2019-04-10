using System.Drawing;
using RGB.NET.Core;
using Stylet;

namespace Artemis.Core.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public interface IModule : IPlugin
    {
        /// <summary>
        ///     Wether or not this module expands upon the main data model. If set to true any data in main data model can be
        ///     accessed by profiles in this module
        /// </summary>
        bool ExpandsMainDataModel { get; }

        /// <summary>
        ///     Called each frame when the module must update
        /// </summary>
        /// <param name="deltaTime">Time since the last update</param>
        void Update(double deltaTime);

        /// <summary>
        ///     Called each frame when the module must render
        /// </summary>
        /// <param name="deltaTime">Time since the last render</param>
        /// <param name="surface">The RGB Surface to render to</param>
        /// <param name="graphics"></param>
        void Render(double deltaTime, RGBSurface surface, Graphics graphics);

        /// <summary>
        ///     Called when the module's main view is being shown
        /// </summary>
        /// <returns></returns>
        IScreen GetMainViewModel();
    }
}