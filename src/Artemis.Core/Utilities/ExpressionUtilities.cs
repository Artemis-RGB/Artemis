using System.Linq.Expressions;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    internal static class ExpressionUtilities
    {
        internal static Expression CreateDataModelAccessor(DataModel dataModel, string path, string parameterName, out ParameterExpression parameter)
        {
            parameter = Expression.Parameter(typeof(object), parameterName + "DataModel");

            // Create an expression that checks every part of the path for null
            // In the same iteration, create the accessor
            Expression source = Expression.Convert(parameter, dataModel.GetType());
            return CreateNullCheckedAccessor(source, path);
        }

        internal static Expression CreateNullCheckedAccessor(Expression source, string path)
        {
            // Create an expression that checks every part of the path for null
            // In the same iteration, create the accessor
            Expression? condition = null;
            foreach (string memberName in path.Split('.'))
            {
                BinaryExpression notNull = Expression.NotEqual(source, Expression.Constant(null));
                condition = condition != null ? Expression.AndAlso(condition, notNull) : notNull;
                source = Expression.PropertyOrField(source, memberName);
            }

            if (condition == null)
                throw new ArtemisCoreException($"Failed to create a null-check for path {path}");

            // Combine the null check and the accessor in a conditional statement that returns the default for the type if null
            return Expression.Condition(condition, source, Expression.Default(source.Type));
        }
    }
}