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
        internal override bool InternalEvaluate(object? leftSideValue, object? rightSideValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TLeftSide leftSide;
            if (leftSideValue != null)
            {
                if (leftSideValue.GetType() != typeof(TLeftSide))
                    leftSide = (TLeftSide) Convert.ChangeType(leftSideValue, typeof(TLeftSide));
                else
                    leftSide = (TLeftSide) leftSideValue;
            }
            else
                leftSide = default;

            TRightSide rightSide;
            if (rightSideValue != null)
            {
                if (rightSideValue.GetType() != typeof(TRightSide))
                    rightSide = (TRightSide) Convert.ChangeType(rightSideValue, typeof(TRightSide));
                else
                    rightSide = (TRightSide) rightSideValue;
            }
            else
                rightSide = default;

            return Evaluate(leftSide!, rightSide!);
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
        internal override bool InternalEvaluate(object? leftSideValue, object? rightSideValue)
        {
            // TODO: Can we avoid boxing/unboxing?
            TLeftSide leftSide;
            if (leftSideValue != null)
            {
                if (leftSideValue.GetType() != typeof(TLeftSide))
                    leftSide = (TLeftSide)Convert.ChangeType(leftSideValue, typeof(TLeftSide));
                else
                    leftSide = (TLeftSide)leftSideValue;
            }
            else
                leftSide = default;

            return Evaluate(leftSide!);
        }

        /// <inheritdoc />
        public override Type LeftSideType => typeof(TLeftSide);

        /// <summary>
        ///     Always <c>null</c>, not applicable to this type of condition operator
        /// </summary>
        public override Type? RightSideType => null;
    }
}