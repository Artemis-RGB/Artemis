using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Artemis.Core.Models.Profile.Conditions.Operators
{
    public class GreaterThanConditionOperator : LayerConditionOperator
    {
        public override IReadOnlyCollection<Type> CompatibleTypes => new List<Type>
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public override BinaryExpression CreateExpression(Type leftSideType, Type rightSideType)
        {
            var leftSideParameter = Expression.Parameter(leftSideType, "a");
            var rightSideParameter = Expression.Parameter(rightSideType, "b");
            return Expression.GreaterThan(leftSideParameter, rightSideParameter);
        }
    }
}