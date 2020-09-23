using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    /// <summary>
    ///     A condition operator is used by the conditions system to perform a specific boolean check
    /// </summary>
    public abstract class ConditionOperator
    {
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

        /// <summary>
        ///     Returns whether the given type is supported by the operator
        /// </summary>
        public bool SupportsType(Type type)
        {
            if (type == null)
                return true;
            return CompatibleTypes.Any(t => t.IsCastableFrom(type));
        }

        /// <summary>
        /// Evaluates the operator on a and b
        /// </summary>
        /// <param name="a">The parameter on the left side of the expression</param>
        /// <param name="b">The parameter on the right side of the expression</param>
        public abstract bool Evaluate(object a, object b);
    }
}