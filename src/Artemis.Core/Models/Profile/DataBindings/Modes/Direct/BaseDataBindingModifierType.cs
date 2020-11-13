using System;

namespace Artemis.Core
{
    /// <summary>
    ///     A modifier that changes the source value of a data binding in some way
    ///     <para>
    ///         To implement your own condition operator, inherit <see cref="DataBindingModifierType{TValue, TParameter}" /> or
    ///         <see cref="DataBindingModifierType{TValue}" />
    ///     </para>
    /// </summary>
    public abstract class BaseDataBindingModifierType
    {
        /// <summary>
        ///     Gets the plugin this data binding modifier belongs to
        ///     <para>Note: Not set until after registering</para>
        /// </summary>
        public Plugin Plugin { get; internal set; }

        /// <summary>
        ///     Gets the value type of this modifier type
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        ///     Gets the parameter type of this modifier type. May be null if the modifier type does not support a parameter
        /// </summary>
        public abstract Type? ParameterType { get; }

        /// <summary>
        ///     Gets the name of this modifier
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets or sets the icon of this modifier
        /// </summary>
        public abstract string Icon { get; }

        /// <summary>
        ///     Gets the description of this modifier
        /// </summary>
        public virtual string Description => null;

        /// <summary>
        ///     Gets the category of this modifier
        /// </summary>
        public virtual string Category => null;

        /// <summary>
        ///     Returns whether the given type is supported by the modifier
        /// </summary>
        /// <param name="type">The type to check for, must be either the same or be castable to the target type</param>
        /// <param name="part">Which part of the modifier to check, the value or the parameter</param>
        public bool SupportsType(Type type, ModifierTypePart part)
        {
            if (type == null)
                return true;
            if (part == ModifierTypePart.Value)
                return ValueType.IsCastableFrom(type);
            return ParameterType != null && ParameterType.IsCastableFrom(type);
        }

        /// <summary>
        ///     Applies the modifier to the provided current value
        ///     <para>
        ///         This leaves the caller responsible for the types matching <see cref="ValueType" /> and
        ///         <see cref="ParameterType" />
        ///     </para>
        /// </summary>
        /// <param name="currentValue">The current value before modification, type should match <see cref="ValueType" /></param>
        /// <param name="parameterValue">The parameter to use for the modification, type should match <see cref="ParameterType" /></param>
        /// <returns>The modified value, with a type of <see cref="ValueType" /></returns>
        internal abstract object? InternalApply(object? currentValue, object? parameterValue);
    }

    public enum ModifierTypePart
    {
        Value,
        Parameter
    }
}