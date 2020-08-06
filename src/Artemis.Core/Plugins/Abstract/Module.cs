using System;
using System.Collections.Generic;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Abstract.ViewModels;
using SkiaSharp;

namespace Artemis.Core.Plugins.Abstract
{
    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing your own data model
    /// </summary>
    public abstract class Module<T> : Module where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        /// </summary>
        public T DataModel
        {
            get => (T) InternalDataModel;
            internal set => InternalDataModel = value;
        }

        /// <summary>
        ///     Gets or sets whether this module must also expand the main data model
        ///     <para>
        ///         Note: If expanding the main data model is all you want your plugin to do, create a
        ///         <see cref="BaseDataModelExpansion" /> plugin instead.
        ///     </para>
        /// </summary>
        public bool ExpandsDataModel
        {
            get => InternalExpandsMainDataModel;
            set => InternalExpandsMainDataModel = value;
        }

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new DataModelPropertyAttribute {Name = PluginInfo.Name, Description = PluginInfo.Description};
        }

        internal override void InternalEnablePlugin()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.PluginInfo = PluginInfo;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnablePlugin();
        }

        internal override void InternalDisablePlugin()
        {
            DataModel = null;
            base.InternalDisablePlugin();
        }
    }


    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public abstract class Module : Plugin
    {
        internal DataModel InternalDataModel { get; set; }

        internal bool InternalExpandsMainDataModel { get; set; }

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
        ///     Called each frame when the module must update
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last update</param>
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