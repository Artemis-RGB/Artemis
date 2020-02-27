using System.Collections.Generic;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using SkiaSharp;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public abstract class Module : Plugin
    {
        protected Module(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }

        /// <summary>
        ///     The modules display name that's shown in the menu
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        ///     The modules display icon that's shown in the menu see <see href="https://materialdesignicons.com" /> for available
        ///     icons
        /// </summary>
        public string DisplayIcon { get; set; }

        /// <summary>
        ///     Whether or not this module expands upon the main data model. If set to true any data in main data model can be
        ///     accessed by profiles in this module
        /// </summary>
        public bool ExpandsMainDataModel { get; protected set; }

        /// <summary>
        ///     Called each frame when the module must update
        /// </summary>
        /// <param name="deltaTime">Time since the last update</param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Called each frame when the module must render
        /// </summary>
        /// <param name="deltaTime">Time since the last render</param>
        /// <param name="surface">The RGB Surface to render to</param>
        /// <param name="canvas"></param>
        /// <param name="canvasInfo"></param>
        public abstract void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo);

        /// <summary>
        ///     Called when the module's view model is being show, return view models here to create tabs for them
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<ModuleViewModel> GetViewModels();
    }
}