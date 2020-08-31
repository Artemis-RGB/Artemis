using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core
{
    internal class StringEndsWithConditionOperator : DisplayConditionOperator
    {
        private readonly MethodInfo _endsWith;
        private readonly MethodInfo _toLower;

        public StringEndsWithConditionOperator()
        {
            _toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            _endsWith = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});
        }

        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type> {typeof(string)};

        public override string Description => "Ends with";
        public override string Icon => "ContainEnd";

        public override BinaryExpression CreateExpression(Expression leftSide, Expression rightSide)
        {
            return Expression.Equal(Expression.Call(Expression.Call(leftSide, _toLower), _endsWith, Expression.Call(rightSide, _toLower)), Expression.Constant(true));
        }
    }
}