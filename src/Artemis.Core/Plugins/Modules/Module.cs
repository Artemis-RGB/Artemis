using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;
using SkiaSharp;

namespace Artemis.Core.Modules
{
    /// <summary>
    ///     Allows you to add new data to the Artemis data model
    /// </summary>
    public abstract class Module<T> : Module where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        ///     <para>Note: This default data model is automatically registered and instantiated upon plugin enable</para>
        /// </summary>
        public T DataModel
        {
            get => InternalDataModel as T ?? throw new InvalidOperationException("Internal datamodel does not match the type of the data model");
            internal set => InternalDataModel = value;
        }

        /// <summary>
        ///     Hide the provided property using a lambda expression, e.g. HideProperty(dm => dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to ignore</param>
        public void HideProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            if (!HiddenPropertiesList.Any(p => p.Equals(propertyInfo)))
                HiddenPropertiesList.Add(propertyInfo);
        }

        /// <summary>
        ///     Stop hiding the provided property using a lambda expression, e.g. ShowProperty(dm =>
        ///     dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to stop ignoring</param>
        public void ShowProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            HiddenPropertiesList.RemoveAll(p => p.Equals(propertyInfo));
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
            Deactivate(true);
            base.InternalDisable();
        }
    }


    /// <summary>
    ///     For internal use only, please use <see cref="Module{T}" />.
    /// </summary>
    public abstract class Module : DataModelPluginFeature
    {
        private readonly List<string> _defaultProfilePaths = new();
        private readonly List<ProfileEntity> _defaultProfiles = new();
        private readonly List<string> _pendingDefaultProfilePaths = new();

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        protected internal readonly List<PropertyInfo> HiddenPropertiesList = new();

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
        public List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

        /// <summary>
        ///     Gets or sets the activation requirement mode, defaults to <see cref="ActivationRequirementType.Any" />
        /// </summary>
        public ActivationRequirementType ActivationRequirementMode { get; set; } = ActivationRequirementType.Any;

        /// <summary>
        ///     Gets or sets a boolean indicating whether this module is always available to profiles or only when profiles
        ///     specifically target this module.
        ///     <para>Note: If set to <see langword="true" />, <see cref="ActivationRequirements" /> are not evaluated.</para>
        /// </summary>
        public bool IsAlwaysAvailable { get; set; }

        /// <summary>
        ///     Gets whether updating this module is currently allowed
        /// </summary>
        public bool IsUpdateAllowed => IsActivated && (UpdateDuringActivationOverride || !IsActivatedOverride);

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> HiddenProperties => HiddenPropertiesList.AsReadOnly();

        internal DataModel? InternalDataModel { get; set; }

        /// <summary>
        ///     Gets a list of default profiles, to add a new default profile use <see cref="AddDefaultProfile" />
        /// </summary>
        internal ReadOnlyCollection<ProfileEntity> DefaultProfiles => _defaultProfiles.AsReadOnly();

        /// <summary>
        ///     Called each frame when the module should update
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last update</param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are met or during an override
        /// </summary>
        /// <param name="isOverride">
        ///     If true, the activation was due to an override. This usually means the module was activated
        ///     by the profile editor
        /// </param>
        public virtual void ModuleActivated(bool isOverride)
        {
        }

        /// <summary>
        ///     Called when the <see cref="ActivationRequirements" /> are no longer met or during an override
        /// </summary>
        /// <param name="isOverride">
        ///     If true, the deactivation was due to an override. This usually means the module was deactivated
        ///     by the profile editor
        /// </param>
        public virtual void ModuleDeactivated(bool isOverride)
        {
        }

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

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new() {Name = Plugin.Info.Name, Description = Plugin.Info.Description};
        }

        /// <summary>
        ///     Adds a default profile by reading it from the file found at the provided path
        /// </summary>
        /// <param name="file">A path pointing towards a profile file. May be relative to the plugin directory.</param>
        /// <returns>
        ///     <see langword="true" /> if the default profile was added; <see langword="false" /> if it was not because it is
        ///     already in the list.
        /// </returns>
        protected bool AddDefaultProfile(string file)
        {
            // It can be null if the plugin has not loaded yet...
            if (Plugin == null!)
            {
                if (_pendingDefaultProfilePaths.Contains(file))
                    return false;
                _pendingDefaultProfilePaths.Add(file);
                return true;
            }

            if (!Path.IsPathRooted(file))
                file = Plugin.ResolveRelativePath(file);

            if (_defaultProfilePaths.Contains(file))
                return false;
            _defaultProfilePaths.Add(file);

            // Ensure the file exists
            if (!File.Exists(file))
                throw new ArtemisPluginFeatureException(this, $"Could not find default profile at {file}.");
            // Deserialize and make sure that succeeded
            ProfileEntity? profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(File.ReadAllText(file), ProfileService.ExportSettings);
            if (profileEntity == null)
                throw new ArtemisPluginFeatureException(this, $"Failed to deserialize default profile at {file}.");
            // Ensure the profile ID is unique
            if (_defaultProfiles.Any(d => d.Id == profileEntity.Id))
                throw new ArtemisPluginFeatureException(this, $"Cannot add default profile from {file}, profile ID {profileEntity.Id} already in use.");

            profileEntity.IsFreshImport = true;
            _defaultProfiles.Add(profileEntity);

            return true;
        }


        internal virtual void InternalUpdate(double deltaTime)
        {
            StartUpdateMeasure();
            if (IsUpdateAllowed)
                Update(deltaTime);
            StopUpdateMeasure();
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
}