using System;
using Ninject;

namespace Artemis.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public interface IModule : IPlugin
    {
        /// <summary>
        ///     The type of this module's view model
        /// </summary>
        Type ViewModelType { get; }

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
        void Render(double deltaTime);
    }
}