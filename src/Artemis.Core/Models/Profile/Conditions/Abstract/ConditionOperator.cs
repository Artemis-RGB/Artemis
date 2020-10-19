using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a condition operator that performs a boolean operation using a left- and right-side
    /// </summary>
    public abstract class ConditionOperator<TLeftSide, TRightSide> : BaseConditionOperator
    {
        /// <summary>
        ///     Evaluates the operator on a and b
        /// </summary>
        /// <param name="a">The parameter on the left side of the expression</param>
        /// <param name="b">The parameter on the right side of the expression</param>
        public abstract bool Evaluate(TLeftSide a, TRightSide b);

        /// <inheritdoc />
        public override bool SupportsType(Type type, ConditionParameterSide side)
        {
            if (type == null)
                return true;
            if (side == ConditionParameterSide.Left)
                return LeftSideType.IsCastableFrom(type);
            return RightSideType.IsCastableFrom(type);
        }

        /// <inheritdoc />
        internal override bool InternalEvaluate(object? leftSideValue, object? rightSideValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TLeftSide leftSide;
            if (leftSideValue != null)
                leftSide = (TLeftSide) Convert.ChangeType(leftSideValue, typeof(TLeftSide));
            else
                leftSide = default;

            TRightSide rightSide;
            if (rightSideValue != null)
                rightSide = (TRightSide) Convert.ChangeType(rightSideValue, typeof(TRightSide));
            else
                rightSide = default;

            return Evaluate(leftSide, rightSide);
        }

        /// <inheritdoc />
        public override Type LeftSideType => typeof(TLeftSide);

        /// <inheritdoc />
        public override Type RightSideType => typeof(TRightSide);
    }

    /// <summary>
    ///     Represents a condition operator that performs a boolean operation using only a left side
    /// </summary>
    public abstract class ConditionOperator<TLeftSide> : BaseConditionOperator
    {
        /// <summary>
        ///     Evaluates the operator on a and b
        /// </summary>
        /// <param name="a">The parameter on the left side of the expression</param>
        public abstract bool Evaluate(TLeftSide a);

        /// <inheritdoc />
        public override bool SupportsType(Type type, ConditionParameterSide side)
        {
            if (type == null)
                return true;
            if (side == ConditionParameterSide.Left)
                return LeftSideType.IsCastableFrom(type);
            return false;
        }

        /// <inheritdoc />
        internal override bool InternalEvaluate(object? leftSideValue, object? rightSideValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TLeftSide leftSide;
            if (leftSideValue != null)
                leftSide = (TLeftSide) Convert.ChangeType(leftSideValue, typeof(TLeftSide));
            else
                leftSide = default;

            return Evaluate(leftSide);
        }

        /// <inheritdoc />
        public override Type LeftSideType => typeof(TLeftSide);

        /// <summary>
        ///     Always <c>null</c>, not applicable to this type of condition operator
        /// </summary>
        public override Type? RightSideType => null;
    }
}