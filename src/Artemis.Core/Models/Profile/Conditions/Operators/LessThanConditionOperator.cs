using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core
{
    internal class LessThanConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is less than";
        public override string Icon => "LessThan";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.LessThan(leftSide, rightSide);
        }
    }
}