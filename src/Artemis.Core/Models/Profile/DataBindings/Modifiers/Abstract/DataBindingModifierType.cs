using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;

namespace Artemis.Core
{
    /// <summary>
    ///     A modifier that changes the source value of a data binding in some way
    /// </summary>
    public abstract class DataBindingModifierType
    {
        private IDataBindingService _dataBindingService;
        private bool _registered;

        /// <summary>
        ///     Gets the plugin info this data binding modifier belongs to
        ///     <para>Note: Not set until after registering</para>
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets the types this modifier supports
        /// </summary>
        public abstract IReadOnlyCollection<Type> CompatibleTypes { get; }

        /// <summary>
        ///     Gets or sets the description of this modifier
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets or sets the icon of this modifier
        /// </summary>
        public abstract string Icon { get; }

        /// <summary>
        ///     Gets or sets whether this modifier supports a parameter, defaults to true
        /// </summary>
        public bool SupportsParameter { get; protected set; } = true;

        /// <summary>
        ///     Returns whether the given type is supported by the modifier
        /// </summary>
        public bool SupportsType(Type type)
        {
            if (type == null)
                return true;
            return CompatibleTypes.Any(t => t.IsCastableFrom(type));
        }

        /// <summary>
        ///     Called whenever the modifier must apply to a specific value, must be a value of a type contained in
        ///     <see cref="CompatibleTypes" />
        /// </summary>
        /// <param name="currentValue">
        ///     The current value before modification, is always of a type contained in
        ///     <see cref="CompatibleTypes" />
        /// </param>
        /// <param name="parameterValue">
        ///     The parameter to use for the modification, is always of a type contained in
        ///     <see cref="CompatibleTypes" />
        /// </param>
        /// <returns>The modified value, must be a value of a type contained in <see cref="CompatibleTypes" /></returns>
        public abstract object Apply(object currentValue, object parameterValue);

        internal void Register(PluginInfo pluginInfo, IDataBindingService dataBindingService)
        {
            if (_registered)
                return;

            PluginInfo = pluginInfo;
            _dataBindingService = dataBindingService;

            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.PluginDisabled += InstanceOnPluginDisabled;

            _registered = true;
        }

        internal void Unsubscribe()
        {
            if (!_registered)
                return;

            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.PluginDisabled -= InstanceOnPluginDisabled;
            _registered = false;
        }

        private void InstanceOnPluginDisabled(object sender, EventArgs e)
        {
            // The service will call Unsubscribe
            _dataBindingService.RemoveModifierType(this);
        }
    }
}