using System;
using System.Collections.Generic;
using System.Reflection;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="LayerProperty{T}" /> instead.
    /// </summary>
    public abstract class BaseLayerProperty
    {
        protected readonly List<DataBinding> _dataBindings = new List<DataBinding>();
        protected readonly List<DataBindingRegistration> _dataBindingRegistrations = new List<DataBindingRegistration>();
        private bool _isHidden;
        private bool _keyframesEnabled;

        internal BaseLayerProperty()
        {
        }

        /// <summary>
        ///     Gets a list containing the active data bindings
        /// </summary>
        public IReadOnlyList<DataBinding> DataBindings => _dataBindings.AsReadOnly();

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this effect is applied to
        /// </summary>
        public RenderProfileElement ProfileElement { get; internal set; }

        /// <summary>
        ///     The parent group of this layer property, set after construction
        /// </summary>
        public LayerPropertyGroup Parent { get; internal set; }

        /// <summary>
        ///     Gets whether keyframes are supported on this type of property
        /// </summary>
        public bool KeyframesSupported { get; protected internal set; } = true;

        /// <summary>
        ///     Gets whether data bindings are supported on this type of property
        /// </summary>
        public bool DataBindingsSupported { get; protected internal set; } = true;

        /// <summary>
        ///     Gets or sets whether keyframes are enabled on this property, has no effect if <see cref="KeyframesSupported" /> is
        ///     False
        /// </summary>
        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                if (_keyframesEnabled == value) return;
                _keyframesEnabled = value;
                OnKeyframesToggled();
            }
        }

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                _isHidden = value;
                OnVisibilityChanged();
            }
        }

        /// <summary>
        ///     Indicates whether the BaseValue was loaded from storage, useful to check whether a default value must be applied
        /// </summary>
        public bool IsLoadedFromStorage { get; internal set; }

        /// <summary>
        ///     Used to declare that this property doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        public bool IsCoreProperty { get; internal set; }

        /// <summary>
        ///     Gets the description attribute applied to this property
        /// </summary>
        public PropertyDescriptionAttribute PropertyDescription { get; internal set; }

        /// <summary>
        ///     Gets a list of all the keyframes in their non-generic base form, without their values being available
        /// </summary>
        public abstract IReadOnlyList<BaseLayerPropertyKeyframe> BaseKeyframes { get; }

        internal PropertyEntity PropertyEntity { get; set; }
        internal LayerPropertyGroup LayerPropertyGroup { get; set; }

        /// <summary>
        ///     Overrides the property value with the default value
        /// </summary>
        public abstract void ApplyDefaultValue();

        /// <summary>
        ///     Returns the type of the property
        /// </summary>
        public abstract Type GetPropertyType();

        /// <summary>
        ///     Applies the provided property entity to the layer property by deserializing the JSON base value and keyframe values
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerPropertyGroup"></param>
        /// <param name="fromStorage"></param>
        internal abstract void ApplyToLayerProperty(PropertyEntity entity, LayerPropertyGroup layerPropertyGroup, bool fromStorage);

        /// <summary>
        ///     Saves the property to the underlying property entity that was configured when calling
        ///     <see cref="ApplyToLayerProperty" />
        /// </summary>
        internal abstract void ApplyToEntity();

        #region Data bindings

        internal DataBindingRegistration RegisterDataBindingProperty(PropertyInfo property, IDataBindingConverter converter)
        {
            var registration = new DataBindingRegistration(this, property, converter);
            _dataBindingRegistrations.Add(registration);
            return registration;
        }

        internal void InitializeDataBindings(IDataModelService dataModelService, IDataBindingService dataBindingService)
        {
            foreach (var dataBinding in DataBindings)
                dataBinding.Initialize(dataModelService, dataBindingService);
        }

        /// <summary>
        ///     Adds a new data binding targeting the given property to the <see cref="DataBindings" /> collection
        /// </summary>
        /// <returns>The newly created data binding</returns>
        public DataBinding EnableDataBinding(DataBindingRegistration dataBindingRegistration)
        {
            var dataBinding = new DataBinding(this, dataBindingProperty);
            _dataBindings.Add(dataBinding);

            return dataBinding;
        }

        /// <summary>
        ///     Removes the provided data binding from the <see cref="DataBindings" /> collection
        /// </summary>
        /// <param name="dataBinding">The data binding to remove</param>
        public void DisableDataBinding(DataBinding dataBinding)
        {
            _dataBindings.Remove(dataBinding);
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs once every frame when the layer property is updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> Updated;

        /// <summary>
        ///     Occurs when the base value of the layer property was updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> BaseValueChanged;

        /// <summary>
        ///     Occurs when the <see cref="IsHidden" /> value of the layer property was updated
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> VisibilityChanged;

        /// <summary>
        ///     Occurs when keyframes are enabled/disabled
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> KeyframesToggled;

        /// <summary>
        ///     Occurs when a new keyframe was added to the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> KeyframeAdded;

        /// <summary>
        ///     Occurs when a keyframe was removed from the layer property
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> KeyframeRemoved;

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        protected virtual void OnBaseValueChanged()
        {
            BaseValueChanged?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        protected virtual void OnKeyframesToggled()
        {
            KeyframesToggled?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        protected virtual void OnKeyframeAdded()
        {
            KeyframeAdded?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        protected virtual void OnKeyframeRemoved()
        {
            KeyframeRemoved?.Invoke(this, new LayerPropertyEventArgs(this));
        }

        #endregion
    }
}