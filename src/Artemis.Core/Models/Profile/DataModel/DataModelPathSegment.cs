using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;
using Humanizer;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a segment of a data model path
    /// </summary>
    public class DataModelPathSegment
    {
        internal DataModelPathSegment(DataModelPath dataModelPath, string identifier, string path)
        {
            DataModelPath = dataModelPath;
            Identifier = identifier;
            Path = path;
        }

        /// <summary>
        ///     Gets the data model path this is a segment of
        /// </summary>
        public DataModelPath DataModelPath { get; }

        /// <summary>
        ///     Gets the identifier that is associated with this segment
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the path that leads to this segment
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Gets the type of data model this segment of the path points to
        /// </summary>
        public DataModelPathSegmentType Type { get; private set; }

        /// <summary>
        ///     Gets the type of dynamic data model this path points to
        ///     <para>Not used if the <see cref="Type" /> is <see cref="DataModelPathSegmentType.Static" /></para>
        /// </summary>
        public Type DynamicDataModelType { get; private set; }

        /// <summary>
        ///     Gets the previous segment in the path
        /// </summary>
        public DataModelPathSegment Previous => Node.Previous?.Value;

        /// <summary>
        ///     Gets the next segment in the path
        /// </summary>
        public DataModelPathSegment Next => Node.Next?.Value;

        internal Func<object, object> Accessor { get; set; }
        internal LinkedListNode<DataModelPathSegment> Node { get; set; }

        /// <summary>
        ///     Returns the current value of the path up to this segment
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return Type == DataModelPathSegmentType.Invalid ? null : Accessor(DataModelPath.Target);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Type}] {Path}";
        }

        /// <summary>
        ///     Gets the property info of the property this segment points to
        /// </summary>
        /// <returns>If static, the property info. If dynamic, <c>null</c></returns>
        public PropertyInfo GetPropertyInfo()
        {
            // Dynamic types have no property and therefore no property info
            if (Type == DataModelPathSegmentType.Dynamic)
                return null;

            // If this is the first segment in a path, the property is located on the data model
            if (Previous == null)
                return DataModelPath.Target.GetType().GetProperty(Identifier);
            // If this is not the first segment in a path, the property is located on the previous segment
            return Previous.GetValue()?.GetType().GetProperty(Identifier);
        }

        /// <summary>
        ///     Gets the property description of the property this segment points to
        /// </summary>
        /// <returns>If found, the data model property description</returns>
        public DataModelPropertyAttribute GetPropertyDescription()
        {
            // Dynamic types have a data model description
            if (Type == DataModelPathSegmentType.Dynamic)
                return ((DataModel) GetValue())?.DataModelDescription;

            var propertyInfo = GetPropertyInfo();
            if (propertyInfo == null)
                return null;

            // Static types may have one as an attribute
            return (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute)) ??
                   new DataModelPropertyAttribute {Name = propertyInfo.Name.Humanize(), ResetsDepth = false};
        }

        /// <summary>
        ///     Gets the type of the property this path points to
        /// </summary>
        /// <returns>If possible, the property type</returns>
        public Type GetPropertyType()
        {
            // Prefer basing the type on the property info
            var propertyInfo = GetPropertyInfo();
            var type = propertyInfo?.PropertyType;
            // Property info is not available on dynamic paths though, so fall back on the current value
            if (propertyInfo == null)
            {
                var currentValue = GetValue();
                if (currentValue != null)
                    type = currentValue.GetType();
            }

            return type;
        }

        internal Expression Initialize(ParameterExpression parameter, Expression expression, Expression nullCondition)
        {
            var previousValue = Previous != null ? Previous.GetValue() : DataModelPath.Target;
            if (previousValue == null)
            {
                Type = DataModelPathSegmentType.Invalid;
                return null;
            }

            // Determine this segment's type by looking for a dynamic data model with the identifier
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
            if (Type == DataModelPathSegmentType.Invalid)
            {
                Accessor = null;
                return null;
            }

            Expression accessorExpression;
            // A static segment just needs to access the property or filed
            if (Type == DataModelPathSegmentType.Static)
                accessorExpression = Expression.PropertyOrField(expression, Identifier);
            // A dynamic segment calls the generic method DataModel.DynamicChild<T> and provides the identifier as an argument
            else
            {
                accessorExpression = Expression.Call(
                    expression,
                    nameof(DataModel.DynamicChild),
                    new[] {DynamicDataModelType},
                    Expression.Constant(Identifier)
                );
            }

            Accessor = Expression.Lambda<Func<object, object>>(
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
            Type = DataModelPathSegmentType.Dynamic;
            DynamicDataModelType = dynamicDataModel.GetType();
        }

        private void DetermineStaticType(object previous)
        {
            var previousType = previous.GetType();
            var property = previousType.GetProperty(Identifier, BindingFlags.Public | BindingFlags.Instance);
            Type = property == null ? DataModelPathSegmentType.Invalid : DataModelPathSegmentType.Static;
        }
    }
}