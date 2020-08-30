using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    internal class LessThanOrEqualConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is less than or equal to";
        public override string Icon => "LessThanOrEqual";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.LessThanOrEqual(leftSide, rightSide);
        }
    }
}