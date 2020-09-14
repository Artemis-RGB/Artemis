using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    /// <summary>
    ///     A modifier that changes the source value of a data binding in some way
    /// </summary>
    public abstract class DataBindingModifierType
    {
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
        ///     Gets or sets whether this modifier supports a parameter, defaults to <c>true</c>
        /// </summary>
        public bool SupportsParameter { get; protected set; } = true;

        /// <summary>
        ///     Gets or sets the preferred parameter type
        ///     <para>If <c>null</c>, the parameter type will match the source property</para>
        /// </summary>
        public Type PreferredParameterType { get; protected set; } = null;

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
    }
}