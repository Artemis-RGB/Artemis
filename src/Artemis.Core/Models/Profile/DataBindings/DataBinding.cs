using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     A data binding that binds a certain <see cref="BaseLayerProperty" /> to a value inside a <see cref="DataModel" />
    /// </summary>
    public class DataBinding
    {
        private readonly List<DataBindingModifier> _modifiers = new List<DataBindingModifier>();

        public DataBinding(BaseLayerProperty layerProperty, PropertyInfo targetProperty)
        {
            LayerProperty = layerProperty;
            TargetProperty = targetProperty;
        }

        public DataBinding(BaseLayerProperty layerProperty, PropertyInfo targetProperty, DataBindingEntity entity)
        {
            LayerProperty = layerProperty;
            TargetProperty = targetProperty;
            Entity = entity;
        }

        /// <summary>
        ///     Gets the layer property this data binding targets
        /// </summary>
        public BaseLayerProperty LayerProperty { get; }

        /// <summary>
        ///     Gets the inner property this data binding targets
        /// </summary>
        public PropertyInfo TargetProperty { get; }

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

        /// <summary>
        /// Gets the compiled function that gets the current value of the data binding target
        /// </summary>
        public Func<DataModel, object> CompiledTargetAccessor { get; private set; }

        internal DataBindingEntity Entity { get; }

        /// <summary>
        ///     Adds a modifier to the data binding's <see cref="Modifiers" /> collection
        /// </summary>
        public void AddModifier(DataBindingModifier modifier)
        {
            if (!_modifiers.Contains(modifier))
            {
                modifier.DataBinding = this;
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
                _modifiers.Remove(modifier);
            }
        }

        /// <summary>
        ///     Updates the source of the data binding and re-compiles the expression
        /// </summary>
        /// <param name="dataModel">The data model of the source</param>
        /// <param name="path">The path pointing to the source inside the data model</param>
        public void UpdateSource(DataModel dataModel, string path)
        {
            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            if (dataModel != null)
            {
                if (!dataModel.ContainsPath(path))
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a property at path '{path}'");
            }

            SourceDataModel = dataModel;
            SourcePropertyPath = path;
            CreateExpression();
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

        internal void Initialize(IDataModelService dataModelService, IDataBindingService dataBindingService)
        {
            // Source

            // Modifiers


            foreach (var dataBindingModifier in Modifiers) 
                dataBindingModifier.Initialize(dataModelService, dataBindingService);
        }

        private void CreateExpression()
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