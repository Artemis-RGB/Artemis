using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core
{
    internal class StringEqualsConditionOperator : DisplayConditionOperator
    {
        private readonly MethodInfo _toLower;

        public StringEqualsConditionOperator()
        {
            _toLower = typeof(string).GetMethod("ToLower", new Type[] { });
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Equals";
        public override string Icon => "Equal";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(Expression.Call(leftSide, _toLower), Expression.Call(rightSide, _toLower));
        }
    }
}