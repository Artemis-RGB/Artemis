using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Models.Profile.Conditions
{
    public abstract class DisplayConditionOperator
    {
        private IDataModelService _dataModelService;
        private bool _registered;

        /// <summary>
        ///     Gets the plugin info this condition operator belongs to
        ///     <para>Note: Not set until after registering</para>
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets the types this operator supports
        /// </summary>
        public abstract IReadOnlyCollection<Type> CompatibleTypes { get; }

        /// <summary>
        ///     Gets or sets the description of this logical operator
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets or sets the icon of this logical operator
        /// </summary>
        public abstract string Icon { get; }

        /// <summary>
        ///     Gets or sets whether this condition operator supports a right side, defaults to true
        /// </summary>
        public bool SupportsRightSide { get; protected set; } = true;

        public bool SupportsType(Type type)
        {
            if (type == null)
                return true;
            return CompatibleTypes.Any(t => t.IsCastableFrom(type));
        }

        /// <summary>
        ///     Creates a binary expression comparing two types
        /// </summary>
        /// <param name="leftSide">The parameter on the left side of the expression</param>
        /// <param name="rightSide">The parameter on the right side of the expression</param>
        /// <returns></returns>
        public abstract BinaryExpression CreateExpression(Expression leftSide, Expression rightSide);

        /// <summary>
        ///     Returns an expression that checks the given expression for null
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected Expression CreateNullCheckExpression(Expression expression)
        {
            return Expression.NotEqual(expression, Expression.Constant(null));
        }

        internal void Register(PluginInfo pluginInfo, IDataModelService dataModelService)
        {
            if (_registered)
                return;

            PluginInfo = pluginInfo;
            _dataModelService = dataModelService;

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
            // Profile editor service will call Unsubscribe
            _dataModelService.RemoveConditionOperator(this);
        }
    }
}