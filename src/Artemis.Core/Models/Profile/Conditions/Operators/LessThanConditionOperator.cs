using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    public class LessThanConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => NumberTypes;

        public override string Description => "Is less than";
        public override string Icon => "LessThan";

        public override BinaryExpression CreateExpression(Type leftSideType, Type rightSideType)
        {
            var leftSideParameter = Expression.Parameter(leftSideType, "a");
            var rightSideParameter = Expression.Parameter(rightSideType, "b");
            return Expression.LessThan(leftSideParameter, rightSideParameter);
        }
    }
}