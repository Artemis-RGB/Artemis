using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
        ///     Hide the provided property using a lambda expression, e.g.
        ///     <c>HideProperty(dm => dm.TimeDataModel.CurrentTimeUTC)</c>
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to ignore</param>
        public void HideProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            if (!HiddenPropertiesList.Any(p => p.Equals(propertyInfo)))
                HiddenPropertiesList.Add(propertyInfo);
        }

        /// <summary>
        ///     Stop hiding the provided property using a lambda expression, e.g.
        ///     <c>ShowProperty(dm => dm.TimeDataModel.CurrentTimeUTC)</c>
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to stop ignoring</param>
        public void ShowProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            HiddenPropertiesList.RemoveAll(p => p.Equals(propertyInfo));
        }

        /// <summary>
        ///     Determines whether the provided dot-separated path is actively being used by Artemis
        ///     <para>Note: <see cref="IsPropertyInUse" /> is slightly faster but string-based.</para>
        /// </summary>
        /// <param name="propertyLambda">
        ///     The path to check per example: <c>IsPropertyInUse(dm => dm.TimeDataModel.CurrentTimeUTC)</c>
        /// </param>
        /// <param name="includeChildren">
        ///     If <see langword="true" /> any child of the given path will return true as well; if
        ///     <see langword="false" /> only an exact path match returns <see langword="true" />.
        /// </param>
        public bool IsPropertyInUse<TProperty>(Expression<Func<T, TProperty>> propertyLambda, bool includeChildren)
        {
            string path = GetMemberPath((MemberExpression) propertyLambda.Body);
            return IsPropertyInUse(path, includeChildren);
        }

        /// <summary>
        ///     Determines whether the provided dot-separated path is actively being used by Artemis
        /// </summary>
        /// <param name="path">The path to check per example: <c>MyDataModelChild.MyDataModelProperty</c></param>
        /// <param name="includeChildren">
        ///     If <see langword="true" /> any child of the given path will return true as well; if
        ///     <see langword="false" /> only an exact path match returns <see langword="true" />.
        /// </param>
        public bool IsPropertyInUse(string path, bool includeChildren)
        {
            return DataModel.IsPropertyInUse(path, includeChildren);
        }

        internal override void InternalEnable()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.Module = this;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnable();
        }

        internal override void InternalDisable()
        {
            Deactivate(true);
            base.InternalDisable();
        }

        private static string GetMemberPath(MemberExpression? me)
        {
            StringBuilder builder = new();
            while (me != null)
            {
                builder.Insert(0, me.Member.Name);
                me = me.Expression as MemberExpression;
                if (me != null)
                    builder.Insert(0, ".");
            }

            return builder.ToString();
        }
    }

    /// <summary>
    ///     For internal use only, please use <see cref="Module{T}" />.
    /// </summary>
    public abstract class Module : PluginFeature
    {
        private readonly List<(DefaultCategoryName, string)> _defaultProfilePaths = new();
        private readonly List<(DefaultCategoryName, string)> _pendingDefaultProfilePaths = new();

        protected Module()
        {
            DefaultProfilePaths = new ReadOnlyCollection<(DefaultCategoryName, string)>(_defaultProfilePaths);
            HiddenProperties = new ReadOnlyCollection<PropertyInfo>(HiddenPropertiesList);
        }

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        protected internal readonly List<PropertyInfo> HiddenPropertiesList = new();

        /// <summary>
        ///     Gets a read only collection of default profile paths
        /// </summary>
        public IReadOnlyCollection<(DefaultCategoryName, string)> DefaultProfilePaths { get; }

        /// <summary>
        ///     A list of activation requirements
        ///     <para>
        ///         If this list is not <see langword="null" /> and not empty <see cref="IsAlwaysAvailable" /> becomes
        ///         <see langword="false" /> and the data of this module is only available to profiles specifically targeting it.
        ///     </para>
        /// </summary>
        public abstract List<IModuleActivationRequirement>? ActivationRequirements { get; }

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
        ///     Gets or sets the activation requirement mode, defaults to <see cref="ActivationRequirementType.Any" />
        /// </summary>
        public ActivationRequirementType ActivationRequirementMode { get; set; } = ActivationRequirementType.Any;

        /// <summary>
        ///     Gets a boolean indicating whether this module is always available to profiles or only to profiles that specifically
        ///     target this module.
        ///     <para>
        ///         Note: <see langword="true" /> if there are any <see cref="ActivationRequirements" />; otherwise
        ///         <see langword="false" />
        ///     </para>
        /// </summary>
        public bool IsAlwaysAvailable => ActivationRequirements == null || ActivationRequirements.Count == 0;

        /// <summary>
        ///     Gets whether updating this module is currently allowed
        /// </summary>
        public bool IsUpdateAllowed => IsActivated && (UpdateDuringActivationOverride || !IsActivatedOverride);

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> HiddenProperties { get; }

        internal DataModel? InternalDataModel { get; set; }

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
            if (IsAlwaysAvailable)
                return true;
            if (ActivationRequirementMode == ActivationRequirementType.All)
                return ActivationRequirements!.All(r => r.Evaluate());
            if (ActivationRequirementMode == ActivationRequirementType.Any)
                return ActivationRequirements!.Any(r => r.Evaluate());

            return false;
        }

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new DataModelPropertyAttribute {Name = Info.Name, Description = Info.Description};
        }

        /// <summary>
        ///     Adds a default profile by reading it from the file found at the provided path
        /// </summary>
        /// <param name="category">The category in which to place the default profile</param>
        /// <param name="file">A path pointing towards a profile file. May be relative to the plugin directory.</param>
        /// <returns>
        ///     <see langword="true" /> if the default profile was added; <see langword="false" /> if it was not because it is
        ///     already in the list.
        /// </returns>
        protected bool AddDefaultProfile(DefaultCategoryName category, string file)
        {
            // It can be null if the plugin has not loaded yet in which case Plugin.ResolveRelativePath fails
            if (Plugin == null!)
            {
                if (_pendingDefaultProfilePaths.Contains((category, file)))
                    return false;
                _pendingDefaultProfilePaths.Add((category, file));
                return true;
            }

            if (!Path.IsPathRooted(file))
                file = Plugin.ResolveRelativePath(file);

            // Ensure the file exists
            if (!File.Exists(file))
                throw new ArtemisPluginFeatureException(this, $"Could not find default profile at {file}.");

            if (_defaultProfilePaths.Contains((category, file)))
                return false;
            _defaultProfilePaths.Add((category, file));

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

        #region Overrides of PluginFeature

        /// <inheritdoc />
        internal override void InternalEnable()
        {
            foreach ((DefaultCategoryName categoryName, var path) in _pendingDefaultProfilePaths)
                AddDefaultProfile(categoryName, path);
            _pendingDefaultProfilePaths.Clear();

            base.InternalEnable();
        }

        #endregion

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