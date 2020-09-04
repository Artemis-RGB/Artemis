using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        ///     Creates a binary expression comparing two types
        /// </summary>
        /// <param name="currentValue">The current value of the data binding</param>
        /// <param name="modifierArgument">An argument passed to the modifier, either static of dynamic</param>
        /// <returns></returns>
        public abstract Expression<object> CreateExpression(ParameterExpression currentValue, Expression modifierArgument);

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