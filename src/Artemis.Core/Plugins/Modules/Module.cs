using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Module;
using SkiaSharp;

namespace Artemis.Core.Modules
{
    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing your own data model
    /// </summary>
    public abstract class Module<T> : Module where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        ///     <para>Note: This default data model is automatically registered upon plugin enable</para>
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
        ///         <see cref="DataModelExpansion{T}" /> plugin instead.
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
            return new DataModelPropertyAttribute {Name = Plugin.Info.Name, Description = Plugin.Info.Description};
        }

        internal override void InternalEnable()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.Feature = this;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnable();
        }

        internal override void InternalDisable()
        {
            DataModel = null;
            base.InternalDisable();
        }
    }


    /// <summary>
    ///     Allows you to add support for new games/applications
    /// </summary>
    public abstract class Module : DataModelPluginFeature
    {
        /// <summary>
        ///     The modules display name that's shown in the menu
        /// </summary>
        public string? DisplayName { get; protected set; }

        /// <summary>
        ///     The modules display icon that's shown in the UI accepts:
        ///     <para>
        ///         Either set to the name of a Material Icon see (<see href="https://materialdesignicons.com" /> for available
        ///         icons) or set to a path relative to the plugin folder pointing to a .svg file
        ///     </para>
        /// </summary>
        public string? DisplayIcon { get; set; }

        /// <summary>
        ///     Gets whether this module is activated. A module can only be active while its <see cref="ActivationRequirements" />
        ///     are met
        /// </summary>
        public bool IsActivated { get; internal set; }

        /// <summary>
        ///     Gets whether this module's activation was due to an override, can only be true if <see cref="IsActivated" /> is
        ///     <see langword="true" />
        /// </summary>
        public bool IsActivatedOverride { get; private set; }

        /// <summary>
        ///     Gets whether this module should update while <see cref="IsActivatedOverride" /> is <see langword="true" />. When
        ///     set to <see langword="false" /> <see cref="Update" /> and any timed updates will not get called during an
        ///     activation override.
        ///     <para>Defaults to <see langword="false" /></para>
        /// </summary>
        public bool UpdateDuringActivationOverride { get; protected set; }

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
        ///     Gets the current priority category of this module
        /// </summary>
        public ModulePriorityCategory PriorityCategory { get; internal set; }

        /// <summary>
        ///     Gets the current priority of this module within its priority category
        /// </summary>
        public int Priority { get; internal set; }

        /// <summary>
        ///     A list of custom module tabs that show in the UI
        /// </summary>
        public IEnumerable<ModuleTab>? ModuleTabs { get; protected set; }

        /// <summary>
        ///     Gets whether updating this module is currently allowed
        /// </summary>
        public bool IsUpdateAllowed => IsActivated && (UpdateDuringActivationOverride || !IsActivatedOverride);

        internal DataModel? InternalDataModel { get; set; }

        internal bool InternalExpandsMainDataModel { get; set; }
        internal ModuleSettingsEntity? SettingsEntity { get; set; }

        /// <summary>
        ///     Called each frame when the module should update
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last update</param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Called each frame when the module should render
        /// </summary>
        /// <param name="deltaTime">Time since the last render</param>
        /// <param name="surface">The RGB Surface to render to</param>
        /// <param name="canvas"></param>
        /// <param name="canvasInfo"></param>
        public abstract void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo);

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are met or during an override
        /// </summary>
        /// <param name="isOverride">
        ///     If true, the activation was due to an override. This usually means the module was activated
        ///     by the profile editor
        /// </param>
        public abstract void ModuleActivated(bool isOverride);

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are no longer met or during an override
        /// </summary>
        /// <param name="isOverride">
        ///     If true, the deactivation was due to an override. This usually means the module was deactivated
        ///     by the profile editor
        /// </param>
        public abstract void ModuleDeactivated(bool isOverride);

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

        internal virtual void InternalUpdate(double deltaTime)
        {
            if (IsUpdateAllowed)
                Update(deltaTime);
        }

        internal virtual void InternalRender(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            Render(deltaTime, surface, canvas, canvasInfo);
        }

        internal virtual void Activate(bool isOverride)
        {
            if (IsActivated)
                return;

            IsActivatedOverride = isOverride;
            ModuleActivated(isOverride);
            IsActivated = true;
        }

        internal virtual void Deactivate(bool isOverride)
        {
            if (!IsActivated)
                return;

            IsActivatedOverride = false;
            IsActivated = false;
            ModuleDeactivated(isOverride);
        }

        internal virtual void Reactivate(bool isDeactivateOverride, bool isActivateOverride)
        {
            if (!IsActivated)
                return;

            Deactivate(isDeactivateOverride);
            Activate(isActivateOverride);
        }

        internal void ApplyToEntity()
        {
            if (SettingsEntity == null)
                SettingsEntity = new ModuleSettingsEntity();

            SettingsEntity.ModuleId = Id;
            SettingsEntity.PriorityCategory = (int) PriorityCategory;
            SettingsEntity.Priority = Priority;
        }
    }

    /// <summary>
    ///     Describes in what way the activation requirements of a module must be met
    /// </summary>
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

    /// <summary>
    ///     Describes the priority category of a module
    /// </summary>
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