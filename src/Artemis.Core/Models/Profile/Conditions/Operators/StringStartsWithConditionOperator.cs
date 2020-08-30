using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    internal class StringStartsWithConditionOperator : DisplayConditionOperator
    {
        private readonly MethodInfo _toLower;
        private readonly MethodInfo _startsWith;

        public StringStartsWithConditionOperator()
        {
            _toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            _startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> { typeof(string) };

        public override string Description => "Starts with";
        public override string Icon => "ContainStart";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(Expression.Call(Expression.Call(leftSide, _toLower), _startsWith, Expression.Call(rightSide, _toLower)), Expression.Constant(true));
        }
    }
}