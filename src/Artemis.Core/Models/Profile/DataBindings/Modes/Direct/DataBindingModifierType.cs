using System;

namespace Artemis.Core
{
    /// <summary>
    ///     A modifier that changes the source value of a data binding in some way using a parameter
    /// </summary>
    public abstract class DataBindingModifierType<TValue, TParameter> : BaseDataBindingModifierType
    {
        /// <inheritdoc />
        public override Type ValueType => typeof(TValue);

        /// <inheritdoc />
        public override Type ParameterType => typeof(TParameter);

        /// <summary>
        ///     Called whenever the modifier must apply to a specific value
        /// </summary>
        /// <param name="currentValue">
        ///     The current value before modification
        /// </param>
        /// <param name="parameterValue">
        ///     The parameter to use for the modification
        /// </param>
        /// <returns>The modified value></returns>
        public abstract TValue Apply(TValue currentValue, TParameter parameterValue);

        /// <inheritdoc />
        internal override object? InternalApply(object? currentValue, object? parameterValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TValue current;
            if (currentValue != null)
                current = (TValue) Convert.ChangeType(currentValue, typeof(TValue));
            else
                current = default;

            TParameter parameter;
            if (parameterValue != null)
                parameter = (TParameter) Convert.ChangeType(parameterValue, typeof(TParameter));
            else
                parameter = default;

            return Apply(current!, parameter!);
        }
    }

    /// <summary>
    ///     A modifier that changes the source value of a data binding in some way
    /// </summary>
    public abstract class DataBindingModifierType<TValue> : BaseDataBindingModifierType
    {
        /// <inheritdoc />
        public override Type ValueType => typeof(TValue);

        /// <inheritdoc />
        public override Type? ParameterType => null;

        /// <summary>
        ///     Called whenever the modifier must apply to a specific value
        /// </summary>
        /// <param name="currentValue">
        ///     The current value before modification
        /// </param>
        /// <returns>The modified value</returns>
        public abstract TValue Apply(TValue currentValue);

        /// <inheritdoc />
        internal override object? InternalApply(object? currentValue, object? parameterValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TValue current;
            if (currentValue != null)
                current = (TValue) Convert.ChangeType(currentValue, typeof(TValue));
            else
                current = default;

            return Apply(current!);
        }
    }
}