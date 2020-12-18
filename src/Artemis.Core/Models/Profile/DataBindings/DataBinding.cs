using System;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBinding<TLayerProperty, TProperty> : IDataBinding
    {
        private TProperty _currentValue = default!;
        private bool _disposed;
        private TimeSpan _easingProgress;
        private TProperty _lastAppliedValue = default!;
        private TProperty _previousValue = default!;
        private bool _reapplyValue;

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
        public DataBindingRegistration<TLayerProperty, TProperty>? Registration { get; private set; }

        /// <summary>
        ///     Gets the layer property this data binding targets
        /// </summary>
        public LayerProperty<TLayerProperty> LayerProperty { get; }

        /// <summary>
        ///     Gets the converter used to apply this data binding to the <see cref="LayerProperty" />
        /// </summary>
        public DataBindingConverter<TLayerProperty, TProperty>? Converter { get; private set; }

        /// <summary>
        ///     Gets the data binding mode
        /// </summary>
        public IDataBindingMode<TLayerProperty, TProperty>? DataBindingMode { get; private set; }

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

            TProperty value = DataBindingMode.GetValue(baseValue);

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
        public Type? GetTargetType()
        {
            return Registration?.PropertyExpression.ReturnType;
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
            Converter = dataBindingRegistration.Converter;
            Registration = dataBindingRegistration;

            if (GetTargetType()!.IsValueType)
            {
                if (_currentValue == null)
                    _currentValue = default!;
                if (_previousValue == null)
                    _previousValue = default!;
            }

            Converter?.Initialize(this);
        }

        private TProperty GetInterpolatedValue()
        {
            if (_easingProgress == EasingTime || Converter == null || !Converter.SupportsInterpolate)
                return _currentValue;

            double easingAmount = _easingProgress.TotalSeconds / EasingTime.TotalSeconds;
            return Converter.Interpolate(_previousValue, _currentValue, Easings.Interpolate(easingAmount, EasingFunction));
        }

        /// <summary>
        ///     Updates the smoothing progress of the data binding
        /// </summary>
        /// <param name="timeline">The timeline to apply during update</param>
        public void Update(Timeline timeline)
        {
            // Don't update data bindings if there is no delta, otherwise this creates an inconsistency between
            // data bindings with easing and data bindings without easing (the ones with easing will seemingly not update)
            if (timeline.Delta == TimeSpan.Zero)
                return;

            UpdateWithDelta(timeline.Delta);
        }

        /// <inheritdoc />
        public void UpdateWithDelta(TimeSpan delta)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            // Data bindings cannot go back in time like brushes
            if (delta < TimeSpan.Zero)
                delta = TimeSpan.Zero;

            _easingProgress = _easingProgress.Add(delta);
            if (_easingProgress > EasingTime)
                _easingProgress = EasingTime;

            // Tell Apply() to apply a new value next call
            _reapplyValue = false;
        }

        /// <inheritdoc />
        public void Apply()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (Converter == null)
                return;

            // If Update() has not been called, reapply the previous value
            if (_reapplyValue)
            {
                Converter.ApplyValue(_lastAppliedValue);
                return;
            }

            TProperty converterValue = Converter.GetValue();
            TProperty value = GetValue(converterValue);
            Converter.ApplyValue(value);

            _lastAppliedValue = value;
            _reapplyValue = true;
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
                _disposed = true;

                if (Registration != null)
                    Registration.DataBinding = null;
                DataBindingMode?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

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
            DataBindingRegistration<TLayerProperty, TProperty>? registration = LayerProperty.GetDataBindingRegistration<TProperty>(Entity.TargetExpression);
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

            // Don't save an invalid state
            if (Registration != null)
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