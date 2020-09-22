using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding mode that directly applies a data model value to a data binding
    /// </summary>
    public class DirectDataBinding<TLayerProperty, TProperty> : IDataBindingMode<TLayerProperty, TProperty>
    {
        private readonly List<DataBindingModifier<TLayerProperty, TProperty>> _modifiers = new List<DataBindingModifier<TLayerProperty, TProperty>>();
        private bool _disposed;

        internal DirectDataBinding(DataBinding<TLayerProperty, TProperty> dataBinding, DirectDataBindingEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;

            Initialize();
            Load();
        }

        internal DirectDataBindingEntity Entity { get; }

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
        public IReadOnlyList<DataBindingModifier<TLayerProperty, TProperty>> Modifiers => _modifiers.AsReadOnly();

        /// <summary>
        ///     Gets the compiled function that gets the current value of the data binding target
        /// </summary>
        public Func<DataModel, object> CompiledTargetAccessor { get; private set; }

        /// <inheritdoc />
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <inheritdoc />
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

            if (CompiledTargetAccessor == null)
                return baseValue;

            var dataBindingValue = CompiledTargetAccessor(SourceDataModel);
            foreach (var dataBindingModifier in Modifiers)
                dataBindingValue = dataBindingModifier.Apply(dataBindingValue);

            return DataBinding.Converter.ConvertFromObject(dataBindingValue);
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;

            foreach (var dataBindingModifier in Modifiers)
                dataBindingModifier.Dispose();
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            // Data model is done during Initialize

            // Modifiers
            foreach (var dataBindingModifierEntity in Entity.Modifiers)
                _modifiers.Add(new DataBindingModifier<TLayerProperty, TProperty>(this, dataBindingModifierEntity));

            ApplyOrder();
        }

        /// <inheritdoc />
        public void Save()
        {
            // Data model
            if (SourceDataModel != null)
            {
                Entity.SourceDataModelGuid = SourceDataModel.PluginInfo.Guid;
                Entity.SourcePropertyPath = SourcePropertyPath;
            }

            // Modifiers
            Entity.Modifiers.Clear();
            foreach (var dataBindingModifier in Modifiers)
                dataBindingModifier.Save();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;

            // Source
            if (Entity.SourceDataModelGuid != null && SourceDataModel == null)
            {
                var dataModel = DataModelStore.Get(Entity.SourceDataModelGuid.Value)?.DataModel;
                if (dataModel != null && dataModel.ContainsPath(Entity.SourcePropertyPath))
                    UpdateSource(dataModel, Entity.SourcePropertyPath);
            }
        }

        private void CreateExpression()
        {
            var listType = SourceDataModel.GetListTypeInPath(SourcePropertyPath);
            if (listType != null)
                throw new ArtemisCoreException($"Cannot create a regular accessor at path {SourcePropertyPath} because the path contains a list");

            var parameter = Expression.Parameter(typeof(DataModel), "targetDataModel");
            var accessor = SourcePropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(parameter, SourceDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );

            var returnValue = Expression.Convert(accessor, typeof(object));

            var lambda = Expression.Lambda<Func<DataModel, object>>(returnValue, parameter);
            CompiledTargetAccessor = lambda.Compile();
        }

        #endregion

        #region Source

        /// <summary>
        ///     Returns the type of the source property of this data binding
        /// </summary>
        public Type GetSourceType()
        {
            return SourceDataModel?.GetTypeAtPath(SourcePropertyPath);
        }

        /// <summary>
        ///     Updates the source of the data binding and re-compiles the expression
        /// </summary>
        /// <param name="dataModel">The data model of the source</param>
        /// <param name="path">The path pointing to the source inside the data model</param>
        public void UpdateSource(DataModel dataModel, string path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

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

        #endregion

        #region Modifiers

        /// <summary>
        ///     Adds a modifier to the direct data binding's <see cref="Modifiers" /> collection
        /// </summary>
        /// <param name="type">The type of the parameter, can either be dynamic (based on a data model value) or static</param>
        public DataBindingModifier<TLayerProperty, TProperty> AddModifier(ProfileRightSideType type)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

            var modifier = new DataBindingModifier<TLayerProperty, TProperty>(this, type);
            _modifiers.Add(modifier);

            ApplyOrder();
            OnModifiersUpdated();

            return modifier;
        }

        /// <summary>
        ///     Removes a modifier from the direct data binding's <see cref="Modifiers" /> collection and disposes it
        /// </summary>
        public void RemoveModifier(DataBindingModifier<TLayerProperty, TProperty> modifier)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");
            if (!_modifiers.Contains(modifier))
                return;

            _modifiers.Remove(modifier);
            modifier.Dispose();

            ApplyOrder();
            OnModifiersUpdated();
        }

        internal void ApplyOrder()
        {
            _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            for (var index = 0; index < _modifiers.Count; index++)
            {
                var modifier = _modifiers[index];
                modifier.Order = index + 1;
            }
        }

        #endregion

        #region Event handlers

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            var dataModel = e.Registration.DataModel;
            if (dataModel.PluginInfo.Guid == Entity.SourceDataModelGuid && dataModel.ContainsPath(Entity.SourcePropertyPath))
                UpdateSource(dataModel, Entity.SourcePropertyPath);
        }

        private void DataModelStoreOnDataModelRemoved(object sender, DataModelStoreEvent e)
        {
            if (SourceDataModel != e.Registration.DataModel)
                return;
            SourceDataModel = null;
            CompiledTargetAccessor = null;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a modifier is added or removed
        /// </summary>
        public event EventHandler ModifiersUpdated;

        protected virtual void OnModifiersUpdated()
        {
            ModifiersUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}