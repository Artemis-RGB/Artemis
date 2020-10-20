using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a condition operator that performs a boolean operation
    ///     <para>
    ///         To implement your own condition operator, inherit <see cref="ConditionOperator{TLeftSide, TRightSide}" /> or
    ///         <see cref="ConditionOperator{TLeftSide}" />
    ///     </para>
    /// </summary>
    public abstract class BaseConditionOperator
    {
        /// <summary>
        ///     Gets or sets the description of this logical operator
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets or sets the icon of this logical operator
        /// </summary>
        public abstract string Icon { get; }

        /// <summary>
        ///     Gets the plugin info this condition operator belongs to
        ///     <para>Note: Not set until after registering</para>
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets the left side type of this condition operator
        /// </summary>
        public abstract Type LeftSideType { get; }

        /// <summary>
        ///     Gets the right side type of this condition operator. May be null if the operator does not support a right side
        /// </summary>
        public abstract Type? RightSideType { get; }

        /// <summary>
        ///     Returns whether the given type is supported by the operator
        /// </summary>
        /// <param name="type">The type to check for, must be either the same or be castable to the target type</param>
        /// <param name="side">Which side of the operator to check, left or right</param>
        public bool SupportsType(Type type, ConditionParameterSide side)
        {
            if (type == null)
                return true;
            if (side == ConditionParameterSide.Left)
                return LeftSideType.IsCastableFrom(type);
            return RightSideType != null && RightSideType.IsCastableFrom(type);
        }

        /// <summary>
        ///     Evaluates the condition with the input types being provided as objects
        ///     <para>
        ///         This leaves the caller responsible for the types matching <see cref="LeftSideType" /> and
        ///         <see cref="RightSideType" />
        ///     </para>
        /// </summary>
        /// <param name="leftSideValue">The left side value, type should match <see cref="LeftSideType" /></param>
        /// <param name="rightSideValue">The right side value, type should match <see cref="RightSideType" /></param>
        /// <returns>The result of the boolean condition's evaluation</returns>
        internal abstract bool InternalEvaluate(object? leftSideValue, object? rightSideValue);
    }

    public enum ConditionParameterSide
    {
        Left,
        Right
    }
}