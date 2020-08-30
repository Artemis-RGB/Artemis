using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    internal class StringNullConditionOperator : DisplayConditionOperator
    {
        public StringNullConditionOperator()
        {
            SupportsRightSide = false;
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Is null";
        public override string Icon => "Null";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(leftSide, Expression.Constant(null, leftSide.Type));
        }
    }
}