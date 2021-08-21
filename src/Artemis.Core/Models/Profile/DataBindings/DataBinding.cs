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
            Script = new NodeScript<TProperty>(GetScriptName(), "The value to put into the data binding", LayerProperty.ProfileElement.Profile);

            Save();
        }

        internal DataBinding(LayerProperty<TLayerProperty> layerProperty, DataBindingEntity entity)
        {
            LayerProperty = layerProperty;
            Entity = entity;
            Script = new NodeScript<TProperty>(GetScriptName(), "The value to put into the data binding", LayerProperty.ProfileElement.Profile);

            // Load will add children so be initialized before that
            Load();
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

        public NodeScript<TProperty> Script { get; private set; }

        /// <summary>
        ///     Gets or sets the easing time of the data binding
        /// </summary>
        public TimeSpan EasingTime { get; set; }

        /// <summary>
        ///     Gets ors ets the easing function of the data binding
        /// </summary>
        public Easings.Functions EasingFunction { get; set; }

        /// <summary>
        ///     Gets the data binding entity this data binding uses for persistent storage
        /// </summary>
        public DataBindingEntity Entity { get; }

        /// <summary>
        ///     Gets the current value of the data binding
        /// </summary>
        /// <param name="baseValue">The base value of the property the data binding is applied to</param>
        /// <returns></returns>
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (Converter == null)
                return baseValue;

            Script.Run();

            // If no easing is to be applied simple return whatever the current value is
            if (EasingTime == TimeSpan.Zero || !Converter.SupportsInterpolate)
                return Script.Result;

            // If the value changed, update the current and previous values used for easing
            if (!Equals(Script.Result, _currentValue))
                ResetEasing(Script.Result);

            // Apply interpolation between the previous and current value
            return GetInterpolatedValue();
        }

        /// <summary>
        ///     Returns the type of the target property of this data binding
        /// </summary>
        public Type? GetTargetType()
        {
            return Registration?.Getter.Method.ReturnType;
        }

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

                Script.Dispose();
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
            if (timeline.Delta == TimeSpan.Zero || timeline.IsOverridden)
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

        private string GetScriptName()
        {
            if (LayerProperty.GetAllDataBindingRegistrations().Count == 1)
                return LayerProperty.PropertyDescription.Name ?? LayerProperty.Path;
            return $"{LayerProperty.PropertyDescription.Name ?? LayerProperty.Path} - {Registration?.DisplayName}";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            // General
            DataBindingRegistration<TLayerProperty, TProperty>? registration = LayerProperty.GetDataBindingRegistration<TProperty>(Entity.Identifier);
            if (registration != null)
                ApplyRegistration(registration);

            EasingTime = Entity.EasingTime;
            EasingFunction = (Easings.Functions) Entity.EasingFunction;
        }

        /// <inheritdoc />
        public void LoadNodeScript()
        {
            Script.Dispose();
            Script = Entity.NodeScript != null
                ? new NodeScript<TProperty>(GetScriptName(), "The value to put into the data binding", Entity.NodeScript, LayerProperty.ProfileElement.Profile)
                : new NodeScript<TProperty>(GetScriptName(), "The value to put into the data binding", LayerProperty.ProfileElement.Profile);
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
                Entity.Identifier = Registration.DisplayName;

            Entity.EasingTime = EasingTime;
            Entity.EasingFunction = (int) EasingFunction;

            Script?.Save();
            Entity.NodeScript = Script?.Entity;
        }

        #endregion
    }
}