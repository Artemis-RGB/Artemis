using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    public class GreaterThanConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => Constants.NumberTypes;

        public override string Description => "Is greater than";
        public override string Icon => "GreaterThan";

        public override BinaryExpression CreateExpression(Type leftSideType, Type rightSideType)
        {
            var leftSideParameter = Expression.Parameter(leftSideType, "a");
            var rightSideParameter = Expression.Parameter(rightSideType, "b");
            return Expression.GreaterThan(leftSideParameter, rightSideParameter);
        }
    }
}