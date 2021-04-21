using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;
using Humanizer;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a segment of a data model path
    /// </summary>
    public class DataModelPathSegment : IDisposable
    {
        private Expression<Func<object, object>>? _accessorLambda;
        private DataModel? _dynamicDataModel;
        private Type? _dynamicDataModelType;
        private DataModelPropertyAttribute? _dynamicDataModelAttribute;

        internal DataModelPathSegment(DataModelPath dataModelPath, string identifier, string path)
        {
            DataModelPath = dataModelPath;
            Identifier = identifier;
            Path = path;
            IsStartSegment = !DataModelPath.Segments.Any();
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
        ///     Gets a boolean indicating whether this is the first segment in the path
        /// </summary>
        public bool IsStartSegment { get; }

        /// <summary>
        ///     Gets the type of data model this segment of the path points to
        /// </summary>
        public DataModelPathSegmentType Type { get; private set; }

        /// <summary>
        ///     Gets the previous segment in the path
        /// </summary>
        public DataModelPathSegment? Previous => Node?.Previous?.Value;

        /// <summary>
        ///     Gets the next segment in the path
        /// </summary>
        public DataModelPathSegment? Next => Node?.Next?.Value;

        internal Func<object, object>? Accessor { get; set; }
        internal LinkedListNode<DataModelPathSegment>? Node { get; set; }

        /// <summary>
        ///     Returns the current value of the path up to this segment
        /// </summary>
        /// <returns></returns>
        public object? GetValue()
        {
            if (Type == DataModelPathSegmentType.Invalid || DataModelPath.Target == null || _accessorLambda == null)
                return null;

            // If the accessor has not yet been compiled do it now that it's first required
            if (Accessor == null)
                Accessor = _accessorLambda.Compile();
            return Accessor(DataModelPath.Target);
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
        public PropertyInfo? GetPropertyInfo()
        {
            // Dynamic types have no property and therefore no property info
            if (Type == DataModelPathSegmentType.Dynamic)
                return null;
            // The start segment has none either because it is the datamodel
            if (IsStartSegment)
                return null;

            // If this is not the first segment in a path, the property is located on the previous segment
            return Previous?.GetPropertyType()?.GetProperty(Identifier);
        }

        /// <summary>
        ///     Gets the property description of the property this segment points to
        /// </summary>
        /// <returns>If found, the data model property description</returns>
        public DataModelPropertyAttribute? GetPropertyDescription()
        {
            // Dynamic types have a data model description
            if (Type == DataModelPathSegmentType.Dynamic)
                return _dynamicDataModelAttribute;
            if (IsStartSegment && DataModelPath.Target != null)
                return DataModelPath.Target.DataModelDescription;
            if (IsStartSegment)
                return null;

            PropertyInfo? propertyInfo = GetPropertyInfo();
            if (propertyInfo == null)
                return null;

            // Static types may have one as an attribute
            DataModelPropertyAttribute? attribute = (DataModelPropertyAttribute?) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
            if (attribute != null)
            {
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    attribute.Name = propertyInfo.Name.Humanize();
                return attribute;
            }

            return new DataModelPropertyAttribute {Name = propertyInfo.Name.Humanize(), ResetsDepth = false};
        }

        /// <summary>
        ///     Gets the type of the property this path points to
        /// </summary>
        /// <returns>If possible, the property type</returns>
        public Type? GetPropertyType()
        {
            // The start segment type is always the target type
            if (IsStartSegment)
                return DataModelPath.Target?.GetType();

            // Prefer basing the type on the property info
            PropertyInfo? propertyInfo = GetPropertyInfo();
            Type? type = propertyInfo?.PropertyType;
            // Property info is not available on dynamic paths though, so fall back on the current value
            if (propertyInfo == null)
            {
                object? currentValue = GetValue();
                if (currentValue != null)
                    type = currentValue.GetType();
            }

            return type;
        }

        internal Expression? Initialize(ParameterExpression parameter, Expression expression, Expression nullCondition)
        {
            if (IsStartSegment)
            {
                Type = DataModelPathSegmentType.Static;
                return CreateExpression(parameter, expression, nullCondition);
            }

            Type? previousType = Previous?.GetPropertyType();
            if (previousType == null)
            {
                Type = DataModelPathSegmentType.Invalid;
                return CreateExpression(parameter, expression, nullCondition);
            }

            // Prefer static since that's faster
            DetermineStaticType(previousType);

            // If no static type could be found, check if this is a data model and if so, look for a dynamic type
            if (Type == DataModelPathSegmentType.Invalid && typeof(DataModel).IsAssignableFrom(previousType))
            {
                _dynamicDataModel = Previous?.GetValue() as DataModel;
                // Cannot determine a dynamic type on a null data model, leave the segment invalid
                if (_dynamicDataModel == null)
                    return CreateExpression(parameter, expression, nullCondition);

                // If a dynamic data model is found the use that
                bool hasDynamicChild = _dynamicDataModel.DynamicChildren.TryGetValue(Identifier, out DynamicChild? dynamicChild);
                if (hasDynamicChild && dynamicChild?.BaseValue != null)
                    DetermineDynamicType(dynamicChild.BaseValue, dynamicChild.Attribute);

                _dynamicDataModel.DynamicChildAdded += DynamicChildOnDynamicChildAdded;
                _dynamicDataModel.DynamicChildRemoved += DynamicChildOnDynamicChildRemoved;
            }

            return CreateExpression(parameter, expression, nullCondition);
        }

        private Expression? CreateExpression(ParameterExpression parameter, Expression expression, Expression nullCondition)
        {
            if (Type == DataModelPathSegmentType.Invalid)
            {
                _accessorLambda = null;
                Accessor = null;
                return null;
            }

            Expression accessorExpression;
            // A start segment just accesses the target
            if (IsStartSegment)
                accessorExpression = expression;
            // A static segment just needs to access the property or filed
            else if (Type == DataModelPathSegmentType.Static)
                accessorExpression = Expression.PropertyOrField(expression, Identifier);
            // A dynamic segment calls the generic method DataModel.DynamicChild<T> and provides the identifier as an argument
            else
            {
                accessorExpression = Expression.Call(
                    expression,
                    nameof(DataModel.GetDynamicChildValue),
                    _dynamicDataModelType != null ? new[] { _dynamicDataModelType } : null,
                    Expression.Constant(Identifier)
                );
            }

            _accessorLambda = Expression.Lambda<Func<object, object>>(
                // Wrap with a null check
                Expression.Condition(
                    nullCondition,
                    Expression.Convert(accessorExpression, typeof(object)),
                    Expression.Convert(Expression.Default(accessorExpression.Type), typeof(object))
                ),
                parameter
            );
            Accessor = null;
            return accessorExpression;
        }

        private void DetermineDynamicType(object dynamicDataModel, DataModelPropertyAttribute attribute)
        {
            Type = DataModelPathSegmentType.Dynamic;
            _dynamicDataModelType = dynamicDataModel.GetType();
            _dynamicDataModelAttribute = attribute;
        }

        private void DetermineStaticType(Type previousType)
        {
            PropertyInfo? property = previousType.GetProperty(Identifier, BindingFlags.Public | BindingFlags.Instance);
            Type = property == null ? DataModelPathSegmentType.Invalid : DataModelPathSegmentType.Static;
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dynamicDataModel != null)
                {
                    _dynamicDataModel.DynamicChildAdded -= DynamicChildOnDynamicChildAdded;
                    _dynamicDataModel.DynamicChildRemoved -= DynamicChildOnDynamicChildRemoved;
                }

                Type = DataModelPathSegmentType.Invalid;

                _accessorLambda = null;
                Accessor = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Event handlers

        private void DynamicChildOnDynamicChildAdded(object? sender, DynamicDataModelChildEventArgs e)
        {
            if (e.Key == Identifier)
                DataModelPath.Initialize();
        }

        private void DynamicChildOnDynamicChildRemoved(object? sender, DynamicDataModelChildEventArgs e)
        {
            if (e.DynamicChild.BaseValue == _dynamicDataModel)
                DataModelPath.Initialize();
        }

        #endregion
    }
}