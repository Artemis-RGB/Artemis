using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.ModuleActivationRequirements;
using Artemis.Storage.Entities.Module;
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
        ///         <see cref="BaseDataModelExpansion{T}" /> plugin instead.
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
        ///     Gets whether this module is activated. A module can only be active while its <see cref="ActivationRequirements" />
        ///     are met
        /// </summary>
        public bool IsActivated { get; internal set; }

        /// <summary>
        ///     A list of activation requirements
        ///     <para>Note: if empty the module is always activated</para>
        /// </summary>
        public List<IModuleActivationRequirement> ActivationRequirements { get; } = new List<IModuleActivationRequirement>();

        /// <summary>
        ///     Gets or sets the activation requirement mode, defaults to <see cref="ActivationRequirementType.Any" />
        /// </summary>
        public ActivationRequirementType ActivationRequirementMode { get; set; } = ActivationRequirementType.Any;

        /// <summary>
        ///     Gets or sets the default priority category for this module, defaults to
        ///     <see cref="ModulePriorityCategory.Normal" />
        /// </summary>
        public ModulePriorityCategory DefaultPriorityCategory { get; set; } = ModulePriorityCategory.Normal;

        /// <summary>
        ///     Gets or sets the current priority category of this module
        /// </summary>
        public ModulePriorityCategory PriorityCategory { get; set; }

        /// <summary>
        ///     Gets or sets the current priority of this module within its priority category
        /// </summary>
        public int Priority { get; set; }

        internal DataModel InternalDataModel { get; set; }

        internal bool InternalExpandsMainDataModel { get; set; }
        internal ModuleSettingsEntity Entity { get; set; }

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

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are met
        /// </summary>
        public abstract void ModuleActivated();

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are no longer met
        /// </summary>
        public abstract void ModuleDeactivated();

        /// <summary>
        ///     Evaluates the activation requirements following the <see cref="ActivationRequirementMode" /> and returns the result
        /// </summary>
        /// <returns>The evaluated result of the activation requirements</returns>
        public bool EvaluateActivationRequirements()
        {
            if (!ActivationRequirements.Any())
                return true;
            if (ActivationRequirementMode == ActivationRequirementType.All)
                return ActivationRequirements.All(r => r.Evaluate());
            if (ActivationRequirementMode == ActivationRequirementType.Any)
                return ActivationRequirements.Any(r => r.Evaluate());

            return false;
        }

        internal virtual void Activate()
        {
            if (IsActivated)
                return;

            ModuleActivated();
            IsActivated = true;
        }

        internal virtual void Deactivate()
        {
            if (!IsActivated)
                return;

            IsActivated = false;
            ModuleDeactivated();
        }

        internal void ApplyToEntity()
        {
            if (Entity == null)
                Entity = new ModuleSettingsEntity();

            Entity.PluginGuid = PluginInfo.Guid;
            Entity.PriorityCategory = (int) PriorityCategory;
            Entity.Priority = Priority;
        }
    }

    public enum ActivationRequirementType
    {
        /// <summary>
        ///     Any activation requirement must be met for the module to activate
        /// </summary>
        Any,

        /// <summary>
        ///     All activation requirements must be met for the module to activate
        /// </summary>
        All
    }

    public enum ModulePriorityCategory
    {
        /// <summary>
        ///     Indicates a normal render priority
        /// </summary>
        Normal,

        /// <summary>
        ///     Indicates that the module renders for a specific application/game, rendering on top of normal modules
        /// </summary>
        Application,

        /// <summary>
        ///     Indicates that the module renders an overlay, always rendering on top
        /// </summary>
        Overlay
    }
}