using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core
{
    internal class EqualsConditionOperator : DisplayConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(object)};

        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(leftSide, rightSide);
        }
    }
}