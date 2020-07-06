using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    public class GreaterThanOrEqualConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => NumberTypes;

        public override string Description => "Is greater than or equal to";
        public override string Icon => "GreaterThanOrEqual";

        public override BinaryExpression CreateExpression(Type leftSideType, Type rightSideType)
        {
            var leftSideParameter = Expression.Parameter(leftSideType, "a");
            var rightSideParameter = Expression.Parameter(rightSideType, "b");
            return Expression.GreaterThanOrEqual(leftSideParameter, rightSideParameter);
        }
    }
}