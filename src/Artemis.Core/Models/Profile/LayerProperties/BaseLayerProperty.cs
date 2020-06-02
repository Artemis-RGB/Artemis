using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="LayerProperty{T}" /> instead.
    /// </summary>
    public abstract class BaseLayerProperty
    {
        private bool _keyframesEnabled;
        private bool _isHidden;

        internal BaseLayerProperty()
        {
        }

        /// <summary>
        /// The layer this property applies to
        /// </summary>
        public Layer Layer { get; internal set; }

        /// <summary>
        ///     The parent group of this layer property, set after construction
        /// </summary>
        public LayerPropertyGroup Parent { get; internal set; }

        /// <summary>
        ///     Gets whether keyframes are supported on this property
        /// </summary>
        public bool KeyframesSupported { get; protected set; } = true;

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
        ///     Gets the total progress on the timeline
        /// </summary>
        public TimeSpan TimelineProgress { get; internal set; }

        /// <summary>
        ///     Used to declare that this property doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        public bool IsCoreProperty { get; internal set; }

        public PropertyDescriptionAttribute PropertyDescription { get; internal set; }

        /// <summary>
        ///     Gets a list of all the keyframes in their non-generic base form, without their values being available
        /// </summary>
        public abstract IReadOnlyList<BaseLayerPropertyKeyframe> BaseKeyframes { get; }

        internal PropertyEntity PropertyEntity { get; set; }
        internal LayerPropertyGroup LayerPropertyGroup { get; set; }
        

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

        #region Events

        /// <summary>
        ///     Occurs once every frame when the layer property is updated
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        ///     Occurs when the base value of the layer property was updated
        /// </summary>
        public event EventHandler BaseValueChanged;

        /// <summary>
        ///     Occurs when the <see cref="IsHidden"/> value of the layer property was updated
        /// </summary>
        public event EventHandler VisibilityChanged;

        /// <summary>
        /// Occurs when keyframes are enabled/disabled
        /// </summary>
        public event EventHandler KeyframesToggled;

        /// <summary>
        ///     Occurs when a new keyframe was added to the layer property
        /// </summary>
        public event EventHandler KeyframeAdded;

        /// <summary>
        ///     Occurs when a keyframe was removed from the layer property
        /// </summary>
        public event EventHandler KeyframeRemoved;

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBaseValueChanged()
        {
            BaseValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnKeyframesToggled()
        {
            KeyframesToggled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnKeyframeAdded()
        {
            KeyframeAdded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnKeyframeRemoved()
        {
            KeyframeRemoved?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public abstract void ApplyDefaultValue();


    }
}