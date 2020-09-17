using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    public abstract class PropertyInputViewModel<T> : PropertyInputViewModel
    {
        private bool _inputDragging;
        private T _inputValue;

        protected PropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService)
        {
            LayerProperty = layerProperty;
            ProfileEditorService = profileEditorService;
            LayerProperty.Updated += LayerPropertyOnUpdated;
            LayerProperty.CurrentValueSet += LayerPropertyOnUpdated;
            LayerProperty.DataBindingEnabled += LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled += LayerPropertyOnDataBindingChange;
            UpdateInputValue();
        }

        protected PropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService, IModelValidator validator) : base(validator)
        {
            LayerProperty = layerProperty;
            ProfileEditorService = profileEditorService;
            LayerProperty.Updated += LayerPropertyOnUpdated;
            LayerProperty.CurrentValueSet += LayerPropertyOnUpdated;
            LayerProperty.DataBindingEnabled += LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled += LayerPropertyOnDataBindingChange;
            UpdateInputValue();
        }

        public LayerProperty<T> LayerProperty { get; }
        public IProfileEditorService ProfileEditorService { get; }

        internal override object InternalGuard { get; } = null;

        public bool InputDragging
        {
            get => _inputDragging;
            private set => SetAndNotify(ref _inputDragging, value);
        }

        public T InputValue
        {
            get => _inputValue;
            set
            {
                SetAndNotify(ref _inputValue, value);
                ApplyInputValue();
            }
        }

        public override void Dispose()
        {
            LayerProperty.Updated -= LayerPropertyOnUpdated;
            LayerProperty.CurrentValueSet -= LayerPropertyOnUpdated;
            LayerProperty.DataBindingEnabled -= LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled -= LayerPropertyOnDataBindingChange;
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void OnInputValueApplied()
        {
        }

        protected virtual void OnInputValueChanged()
        {
        }

        protected virtual void OnDataBindingsChanged()
        {
        }

        protected void ApplyInputValue()
        {
            // Force the validator to run
            if (Validator != null)
                Validate();
            // Only apply the input value to the layer property if the validator found no errors
            if (!HasErrors)
                SetCurrentValue(_inputValue, !InputDragging);

            OnInputValueChanged();
            OnInputValueApplied();
        }

        private void LayerPropertyOnUpdated(object sender, EventArgs e)
        {
            UpdateInputValue();
        }

        private void UpdateInputValue()
        {
            // Avoid unnecessary UI updates and validator cycles
            if (_inputValue != null && _inputValue.Equals(LayerProperty.CurrentValue) || _inputValue == null && LayerProperty.CurrentValue == null)
                return;

            // Override the input value
            _inputValue = LayerProperty.CurrentValue;

            // Notify a change in the input value
            OnInputValueChanged();
            NotifyOfPropertyChange(nameof(InputValue));

            // Force the validator to run with the overridden value
            if (Validator != null)
                Validate();
        }

        #region Event handlers

        public void InputDragStarted(object sender, EventArgs e)
        {
            InputDragging = true;
        }

        public void InputDragEnded(object sender, EventArgs e)
        {
            InputDragging = false;
            ProfileEditorService.UpdateSelectedProfileElement();
        }

        private void SetCurrentValue(T value, bool saveChanges)
        {
            LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            if (saveChanges)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
        }

        private void LayerPropertyOnDataBindingChange(object sender, LayerPropertyEventArgs<T> e)
        {
            OnDataBindingsChanged();
        }

        #endregion
    }

    /// <summary>
    ///     For internal use only, implement <see cref="PropertyInputViewModel{T}" /> instead.
    /// </summary>
    public abstract class PropertyInputViewModel : ValidatingModelBase, IDisposable
    {
        protected PropertyInputViewModel()
        {
        }

        protected PropertyInputViewModel(IModelValidator validator) : base(validator)
        {
        }

        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="PropertyInputViewModel{T}" /> instead.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        internal abstract object InternalGuard { get; }

        public abstract void Dispose();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}