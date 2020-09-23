using System;
using System.Collections.Generic;
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
            ApplyDataBindingMode();
        }

        internal DataBinding(LayerProperty<TLayerProperty> layerProperty, DataBindingEntity entity)
        {
            LayerProperty = layerProperty;
            Entity = entity;

            // Load will add children so be initialized before that
            Load();
            ApplyDataBindingMode();
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
        ///     Gets the data binding mode
        /// </summary>
        public IDataBindingMode<TLayerProperty, TProperty> DataBindingMode { get; private set; }

        /// <summary>
        ///     Gets or sets the easing time of the data binding
        /// </summary>
        public TimeSpan EasingTime { get; set; }

        /// <summary>
        ///     Gets ors ets the easing function of the data binding
        /// </summary>
        public Easings.Functions EasingFunction { get; set; }


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

            Registration.DataBinding = null;
            DataBindingMode?.Dispose();
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

            if (Converter == null || DataBindingMode == null)
                return baseValue;

            var value = DataBindingMode.GetValue(baseValue);

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

        private void ResetEasing(TProperty value)
        {
            _previousValue = GetInterpolatedValue();
            _currentValue = value;
            _easingProgress = TimeSpan.Zero;
        }

        private void ApplyRegistration(DataBindingRegistration<TLayerProperty, TProperty> dataBindingRegistration)
        {
            if (dataBindingRegistration == null)
                throw new ArgumentNullException(nameof(dataBindingRegistration));

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

        #region Mode management

        /// <summary>
        ///     Changes the data binding mode of the data binding to the specified <paramref name="dataBindingMode" />
        /// </summary>
        public void ChangeDataBindingMode(DataBindingModeType dataBindingMode)
        {
            switch (dataBindingMode)
            {
                case DataBindingModeType.Direct:
                    Entity.DataBindingMode = new DirectDataBindingEntity();
                    break;
                case DataBindingModeType.Conditional:
                    Entity.DataBindingMode = new ConditionalDataBindingEntity();
                    break;
                default:
                    Entity.DataBindingMode = null;
                    break;
            }

            ApplyDataBindingMode();
        }

        private void ApplyDataBindingMode()
        {
            DataBindingMode?.Dispose();
            DataBindingMode = null;

            switch (Entity.DataBindingMode)
            {
                case DirectDataBindingEntity directDataBindingEntity:
                    DataBindingMode = new DirectDataBinding<TLayerProperty, TProperty>(this, directDataBindingEntity);
                    break;
                case ConditionalDataBindingEntity conditionalDataBindingEntity:
                    DataBindingMode = new ConditionalDataBinding<TLayerProperty, TProperty>(this, conditionalDataBindingEntity);
                    break;
            }
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            // General
            var registration = LayerProperty.GetDataBindingRegistration<TProperty>(Entity.TargetExpression);
            if (registration != null)
                ApplyRegistration(registration);

            EasingTime = Entity.EasingTime;
            EasingFunction = (Easings.Functions) Entity.EasingFunction;

            DataBindingMode?.Load();
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
            Entity.EasingTime = EasingTime;
            Entity.EasingFunction = (int) EasingFunction;

            DataBindingMode?.Save();
        }

        #endregion
    }

    /// <summary>
    ///     A mode that determines how the data binding is applied to the layer property
    /// </summary>
    public enum DataBindingModeType
    {
        /// <summary>
        ///     Disables the data binding
        /// </summary>
        None,

        /// <summary>
        ///     Replaces the layer property value with the data binding value
        /// </summary>
        Direct,

        /// <summary>
        ///     Replaces the layer property value with the data binding value
        /// </summary>
        Conditional
    }
}