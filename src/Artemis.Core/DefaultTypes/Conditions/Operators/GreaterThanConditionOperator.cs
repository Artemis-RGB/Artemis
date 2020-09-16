using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.DefaultTypes
{
    internal class GreaterThanConditionOperator : ConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is greater than";
        public override string Icon => "GreaterThan";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.GreaterThan(leftSide, rightSide);
        }
    }
}