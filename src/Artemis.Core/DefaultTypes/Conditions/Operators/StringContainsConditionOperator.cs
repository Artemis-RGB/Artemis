using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core.DefaultTypes
{
    internal class StringContainsConditionOperator : ConditionOperator
    {
        private readonly MethodInfo _contains;
        private readonly MethodInfo _toLower;

        public StringContainsConditionOperator()
        {
            _toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            _contains = typeof(string).GetMethod("Contains", new[] {typeof(string)});
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Contains";
        public override string Icon => "Contain";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            var contains = Expression.Equal(
                Expression.Call(Expression.Call(leftSide, _toLower), _contains, Expression.Call(rightSide, _toLower)),
                Expression.Constant(true)
            );
            return AddNullChecks(leftSide, rightSide, contains);
        }
    }
}