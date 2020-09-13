using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBinding<TLayerProperty, TProperty> : IDataBinding
    {
        private readonly List<DataBindingModifier<TLayerProperty, TProperty>> _modifiers = new List<DataBindingModifier<TLayerProperty, TProperty>>();

        private TProperty _currentValue;
        private bool _disposed;
        private TimeSpan _easingProgress;
        private TProperty _previousValue;

        internal DataBinding(DataBindingRegistration<TLayerProperty, TProperty> dataBindingRegistration)
        {
            LayerProperty = dataBindingRegistration.LayerProperty;
            Entity = new DataBindingEntity();

            ApplyRegistration(dataBindingRegistration);
            Save();
            Initialize();
        }

        internal DataBinding(LayerProperty<TLayerProperty> layerProperty, DataBindingEntity entity)
        {
            LayerProperty = layerProperty;
            Entity = entity;

            // Load will add children so be initialized before that
            Initialize();
            Load();
        }

        /// <summary>
        ///     Gets the data binding registration this data binding is based upon
        /// </summary>
        public DataBindingRegistration<TLayerProperty, TProperty> Registration { get; private set; }

        /// <summary>
        ///     Gets the layer property this data binding targets
        /// </summary>
        public LayerProperty<TLayerProperty> LayerProperty { get; }

        /// <summary>
        ///     Gets the converter used to apply this data binding to the <see cref="LayerProperty" />
        /// </summary>
        public DataBindingConverter<TLayerProperty, TProperty> Converter { get; private set; }

        /// <summary>
        ///     Gets the currently used instance of the data model that contains the source of the data binding
        /// </summary>
        public DataModel SourceDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the source property in the <see cref="SourceDataModel" />
        /// </summary>
        public string SourcePropertyPath { get; private set; }

        public DataBindingMode Mode { get; set; }

        /// <summary>
        ///     Gets or sets the easing time of the data binding
        /// </summary>
        public TimeSpan EasingTime { get; set; }

        /// <summary>
        ///     Gets ors ets the easing function of the data binding
        /// </summary>
        public Easings.Functions EasingFunction { get; set; }

        /// <summary>
        ///     Gets a list of modifiers applied to this data binding
        /// </summary>
        public IReadOnlyList<DataBindingModifier<TLayerProperty, TProperty>> Modifiers => _modifiers.AsReadOnly();

        /// <summary>
        ///     Gets the compiled function that gets the current value of the data binding target
        /// </summary>
        public Func<DataModel, object> CompiledTargetAccessor { get; private set; }

        internal DataBindingEntity Entity { get; }

        /// <summary>
        ///     Updates the smoothing progress of the data binding
        /// </summary>
        /// <param name="deltaTime">The time in seconds that passed since the last update</param>
        public void Update(double deltaTime)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            // Data bindings cannot go back in time like brushes
            deltaTime = Math.Max(0, deltaTime);

            _easingProgress = _easingProgress.Add(TimeSpan.FromSeconds(deltaTime));
            if (_easingProgress > EasingTime)
                _easingProgress = EasingTime;
        }

        /// <inheritdoc />
        public void Apply()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (Converter == null)
                return;

            var converterValue = Converter.GetValue();
            var value = GetValue(converterValue);
            Converter.ApplyValue(value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;

            foreach (var dataBindingModifier in Modifiers)
                dataBindingModifier.Dispose();
        }

        /// <summary>
        ///     Adds a modifier to the data binding's <see cref="Modifiers" /> collection
        /// </summary>
        public void AddModifier(DataBindingModifier<TLayerProperty, TProperty> modifier)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (!_modifiers.Contains(modifier))
            {
                modifier.DataBinding = this;
                _modifiers.Add(modifier);

                OnModifiersUpdated();
            }
        }

        /// <summary>
        ///     Removes a modifier from the data binding's <see cref="Modifiers" /> collection
        /// </summary>
        public void RemoveModifier(DataBindingModifier<TLayerProperty, TProperty> modifier)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (_modifiers.Contains(modifier))
            {
                modifier.DataBinding = null;
                _modifiers.Remove(modifier);

                OnModifiersUpdated();
            }
        }

        /// <summary>
        ///     Updates the source of the data binding and re-compiles the expression
        /// </summary>
        /// <param name="dataModel">The data model of the source</param>
        /// <param name="path">The path pointing to the source inside the data model</param>
        public void UpdateSource(DataModel dataModel, string path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

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
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (CompiledTargetAccessor == null || Converter == null)
                return baseValue;

            var dataBindingValue = CompiledTargetAccessor(SourceDataModel);
            foreach (var dataBindingModifier in Modifiers)
                dataBindingValue = dataBindingModifier.Apply(dataBindingValue);

            var value = (TProperty) Convert.ChangeType(dataBindingValue, typeof(TProperty));

            // If no easing is to be applied simple return whatever the current value is
            if (EasingTime == TimeSpan.Zero || !Converter.SupportsInterpolate)
                return value;

            // If the value changed, update the current and previous values used for easing
            if (!Equals(value, _currentValue))
                ResetEasing(value);

            // Apply interpolation between the previous and current value
            return GetInterpolatedValue();
        }

        /// <summary>
        ///     Returns the type of the target property of this data binding
        /// </summary>
        public Type GetTargetType()
        {
            return Registration.PropertyExpression.ReturnType;
        }

        /// <summary>
        ///     Returns the type of the source property of this data binding
        /// </summary>
        public Type GetSourceType()
        {
            return SourceDataModel?.GetTypeAtPath(SourcePropertyPath);
        }

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

        private void ResetEasing(TProperty value)
        {
            _previousValue = GetInterpolatedValue();
            _currentValue = value;
            _easingProgress = TimeSpan.Zero;
        }

        private void ApplyRegistration(DataBindingRegistration<TLayerProperty, TProperty> dataBindingRegistration)
        {
            if (dataBindingRegistration != null)
                dataBindingRegistration.DataBinding = this;

            Converter = dataBindingRegistration?.Converter;
            Registration = dataBindingRegistration;

            if (GetTargetType().IsValueType)
            {
                if (_currentValue == null)
                    _currentValue = default;
                if (_previousValue == null)
                    _previousValue = default;
            }

            Converter?.Initialize(this);
        }

        private TProperty GetInterpolatedValue()
        {
            if (_easingProgress == EasingTime || !Converter.SupportsInterpolate)
                return _currentValue;

            var easingAmount = _easingProgress.TotalSeconds / EasingTime.TotalSeconds;
            return Converter.Interpolate(_previousValue, _currentValue, Easings.Interpolate(easingAmount, EasingFunction));
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

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");
            // General
            var registration = LayerProperty.GetDataBindingRegistration<TProperty>(Entity.TargetExpression);
            ApplyRegistration(registration);

            Mode = (DataBindingMode) Entity.DataBindingMode;
            EasingTime = Entity.EasingTime;
            EasingFunction = (Easings.Functions) Entity.EasingFunction;

            // Data model is done during Initialize

            // Modifiers
            foreach (var dataBindingModifierEntity in Entity.Modifiers)
                _modifiers.Add(new DataBindingModifier<TLayerProperty, TProperty>(this, dataBindingModifierEntity));
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (!LayerProperty.Entity.DataBindingEntities.Contains(Entity))
                LayerProperty.Entity.DataBindingEntities.Add(Entity);

            // General 
            Entity.TargetExpression = Registration.PropertyExpression.ToString();
            Entity.DataBindingMode = (int) Mode;
            Entity.EasingTime = EasingTime;
            Entity.EasingFunction = (int) EasingFunction;

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

    /// <summary>
    ///     A mode that determines how the data binding is applied to the layer property
    /// </summary>
    public enum DataBindingMode
    {
        /// <summary>
        ///     Replaces the layer property value with the data binding value
        /// </summary>
        Replace,

        /// <summary>
        ///     Adds the data binding value to the layer property value
        /// </summary>
        Add
    }
}