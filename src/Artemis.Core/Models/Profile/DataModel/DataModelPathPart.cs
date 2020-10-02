using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a part of a data model path
    /// </summary>
    public class DataModelPathPart
    {
        internal DataModelPathPart(DataModelPath dataModelPath, string identifier, string path)
        {
            DataModelPath = dataModelPath;
            Identifier = identifier;
            Path = path;
        }

        /// <summary>
        ///     Gets the data model path this is a part of
        /// </summary>
        public DataModelPath DataModelPath { get; }

        /// <summary>
        ///     Gets the identifier that is associated with this part
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the path that leads to this part
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Gets the type of data model this part of the path points to
        /// </summary>
        public DataModelPathPartType Type { get; private set; }

        /// <summary>
        ///     Gets the type of dynamic data model this path points to
        ///     <para>Not used if the <see cref="Type" /> is <see cref="DataModelPathPartType.Static" /></para>
        /// </summary>
        public Type DynamicDataModelType { get; private set; }

        /// <summary>
        ///     Gets the previous part in the path
        /// </summary>
        public DataModelPathPart Previous => Node.Previous?.Value;

        /// <summary>
        ///     Gets the next part in the path
        /// </summary>
        public DataModelPathPart Next => Node.Next?.Value;

        internal Func<DataModel, object> Accessor { get; set; }
        internal LinkedListNode<DataModelPathPart> Node { get; set; }

        /// <summary>
        ///     Returns the current value of the path up to this part
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return Type == DataModelPathPartType.Invalid ? null : Accessor(DataModelPath.DataModel);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Type}] {Path}";
        }

        internal Expression Initialize(ParameterExpression parameter, Expression expression, Expression nullCondition)
        {
            var previousValue = Previous != null ? Previous.GetValue() : DataModelPath.DataModel;
            if (previousValue == null)
            {
                Type = DataModelPathPartType.Invalid;
                return null;
            }

            // Determine this part's type by looking for a dynamic data model with the identifier
            if (previousValue is DataModel dataModel)
            {
                var hasDynamicDataModel = dataModel.DynamicDataModels.TryGetValue(Identifier, out var dynamicDataModel);
                // If a dynamic data model is found the use that
                if (hasDynamicDataModel)
                    DetermineDynamicType(dynamicDataModel);
                // Otherwise look for a static type
                else
                    DetermineStaticType(previousValue);
            }
            // Only data models can have dynamic types so if it is something else, its always going to be static
            else
                DetermineStaticType(previousValue);

            return CreateExpression(parameter, expression, nullCondition);
        }

        private Expression CreateExpression(ParameterExpression parameter, Expression expression, Expression nullCondition)
        {
            if (Type == DataModelPathPartType.Invalid)
            {
                Accessor = null;
                return null;
            }

            Expression accessorExpression;
            // A static part just needs to access the property or filed
            if (Type == DataModelPathPartType.Static)
                accessorExpression = Expression.PropertyOrField(expression, Identifier);
            // A dynamic part calls the generic method DataModel.DynamicChild<T> and provides the identifier as an argument
            else
            {
                accessorExpression = Expression.Call(
                    expression,
                    nameof(DataModel.DynamicChild),
                    new[] {DynamicDataModelType},
                    Expression.Constant(Identifier)
                );
            }

            Accessor = Expression.Lambda<Func<DataModel, object>>(
                // Wrap with a null check
                Expression.Condition(
                    nullCondition,
                    Expression.Convert(accessorExpression, typeof(object)),
                    Expression.Convert(Expression.Default(accessorExpression.Type), typeof(object))
                ),
                parameter
            ).Compile();

            return accessorExpression;
        }

        private void DetermineDynamicType(DataModel dynamicDataModel)
        {
            Type = DataModelPathPartType.Dynamic;
            DynamicDataModelType = dynamicDataModel.GetType();
        }

        private void DetermineStaticType(object previous)
        {
            var previousType = previous.GetType();
            var property = previousType.GetProperty(Identifier, BindingFlags.Public | BindingFlags.Instance);
            Type = property == null ? DataModelPathPartType.Invalid : DataModelPathPartType.Static;
        }
    }
}