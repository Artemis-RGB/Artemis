using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core
{
    internal class GreaterThanOrEqualConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is greater than or equal to";
        public override string Icon => "GreaterThanOrEqual";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.GreaterThanOrEqual(leftSide, rightSide);
        }
    }
}