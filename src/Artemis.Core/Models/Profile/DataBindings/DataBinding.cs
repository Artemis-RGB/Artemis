using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     A data binding that binds a certain <see cref="BaseLayerProperty" /> to a value inside a <see cref="DataModel" />
    /// </summary>
    public class DataBinding
    {
        private readonly List<DataBindingModifier> _modifiers = new List<DataBindingModifier>();

        /// <summary>
        ///     Gets the layer property this data binding targets
        /// </summary>
        public BaseLayerProperty LayerProperty { get; private set; }

        /// <summary>
        ///     Gets the inner property this data binding targets
        /// </summary>
        public PropertyInfo TargetProperty { get; private set; }

        /// <summary>
        ///     Gets the currently used instance of the data model that contains the source of the data binding
        /// </summary>
        public DataModel SourceDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the source property in the <see cref="SourceDataModel" />
        /// </summary>
        public string SourcePropertyPath { get; private set; }

        /// <summary>
        ///     Gets a list of modifiers applied to this data binding
        /// </summary>
        public IReadOnlyList<DataBindingModifier> Modifiers => _modifiers.AsReadOnly();

        public Func<DataModel, object> CompiledTargetAccessor { get; private set; }

        /// <summary>
        ///     Adds a modifier to the data binding's <see cref="Modifiers" /> collection
        /// </summary>
        public void AddModifier(DataBindingModifier modifier)
        {
            if (!_modifiers.Contains(modifier))
            {
                modifier.DataBinding = this;
                modifier.CreateExpression();
                _modifiers.Add(modifier);
            }
        }

        /// <summary>
        ///     Removes a modifier from the data binding's <see cref="Modifiers" /> collection
        /// </summary>
        public void RemoveModifier(DataBindingModifier modifier)
        {
            if (_modifiers.Contains(modifier))
            {
                modifier.DataBinding = null;
                modifier.CreateExpression();
                _modifiers.Remove(modifier);
            }
        }

        /// <summary>
        ///     Gets the current value of the data binding
        /// </summary>
        /// <param name="baseValue">The base value of the property the data binding is applied to</param>
        /// <returns></returns>
        public object GetValue(object baseValue)
        {
            if (baseValue.GetType() != TargetProperty.PropertyType)
            {
                throw new ArtemisCoreException($"The provided current value type ({baseValue.GetType().Name}) not match the " +
                                               $"target property type ({TargetProperty.PropertyType.Name})");
            }

            if (CompiledTargetAccessor == null)
                return baseValue;

            var dataBindingValue = CompiledTargetAccessor(SourceDataModel);
            foreach (var dataBindingModifier in Modifiers)
                dataBindingValue = dataBindingModifier.Apply(dataBindingValue);

            return dataBindingValue;
        }

        public void Update()
        {
            var listType = SourceDataModel.GetListTypeInPath(SourcePropertyPath);
            if (listType != null)
                throw new ArtemisCoreException($"Cannot create a regular accessor at path {SourcePropertyPath} because the path contains a list");

            var parameter = Expression.Parameter(typeof(object), "targetDataModel");
            var accessor = SourcePropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(parameter, SourceDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );

            var lambda = Expression.Lambda<Func<DataModel, object>>(accessor);
            CompiledTargetAccessor = lambda.Compile();
        }
    }
}