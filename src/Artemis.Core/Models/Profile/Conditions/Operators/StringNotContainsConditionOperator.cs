using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    internal class StringNotContainsConditionOperator : DisplayConditionOperator
    {
        private readonly MethodInfo _toLower;
        private readonly MethodInfo _contains;

        public StringNotContainsConditionOperator()
        {
            _toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            _contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> { typeof(string) };

        public override string Description => "Does not contain";
        public override string Icon => "FormatStrikethrough";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(Expression.Call(Expression.Call(leftSide, _toLower), _contains, Expression.Call(rightSide, _toLower)), Expression.Constant(false));
        }
    }
}