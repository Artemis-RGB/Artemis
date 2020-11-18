using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents the base class for a property input view model that is used to edit layer properties
    /// </summary>
    /// <typeparam name="T">The type of property this input view model supports</typeparam>
    public abstract class PropertyInputViewModel<T> : PropertyInputViewModel
    {
        private bool _inputDragging;
        private T _inputValue;

        /// <summary>
        ///     Creates a new instance of the <see cref="PropertyInputViewModel" /> class
        /// </summary>
        /// <param name="layerProperty">The layer property this view model will edit</param>
        /// <param name="profileEditorService">The profile editor service</param>
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

        /// <summary>
        ///     Creates a new instance of the <see cref="PropertyInputViewModel" /> class
        /// </summary>
        /// <param name="layerProperty">The layer property this view model will edit</param>
        /// <param name="profileEditorService">The profile editor service</param>
        /// <param name="validator">The validator used to validate the input</param>
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

        /// <summary>
        ///     Gets the layer property this view model is editing
        /// </summary>
        public LayerProperty<T> LayerProperty { get; }

        /// <summary>
        ///     Gets the profile editor service
        /// </summary>
        public IProfileEditorService ProfileEditorService { get; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the input is currently being dragged
        ///     <para>
        ///         Only applicable when using something like a <see cref="DraggableFloat" />, see
        ///         <see cref="InputDragStarted" /> and <see cref="InputDragEnded" />
        ///     </para>
        /// </summary>
        public bool InputDragging
        {
            get => _inputDragging;
            private set => SetAndNotify(ref _inputDragging, value);
        }

        /// <summary>
        ///     Gets or sets the input value
        /// </summary>
        public T InputValue
        {
            get => _inputValue;
            set
            {
                SetAndNotify(ref _inputValue, value);
                ApplyInputValue();
            }
        }

        internal override object InternalGuard { get; } = new object();

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LayerProperty.Updated -= LayerPropertyOnUpdated;
                LayerProperty.CurrentValueSet -= LayerPropertyOnUpdated;
                LayerProperty.DataBindingEnabled -= LayerPropertyOnDataBindingChange;
                LayerProperty.DataBindingDisabled -= LayerPropertyOnDataBindingChange;
            }

            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        ///     Called when the input value has been applied to the layer property
        /// </summary>
        protected virtual void OnInputValueApplied()
        {
        }

        /// <summary>
        ///     Called when the input value has changed
        /// </summary>
        protected virtual void OnInputValueChanged()
        {
        }

        /// <summary>
        ///     Called when data bindings have been enabled or disabled on the layer property
        /// </summary>
        protected virtual void OnDataBindingsChanged()
        {
        }

        /// <summary>
        ///     Applies the input value to the layer property
        /// </summary>
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

        private void SetCurrentValue(T value, bool saveChanges)
        {
            LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            if (saveChanges)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
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

        /// <summary>
        ///     Called by the view input drag has started
        ///     <para>
        ///         To use, add the following to DraggableFloat in your xaml: <c>DragStarted="{s:Action InputDragStarted}"</c>
        ///     </para>
        /// </summary>
        public void InputDragStarted(object sender, EventArgs e)
        {
            InputDragging = true;
        }

        /// <summary>
        ///     Called by the view when input drag has ended
        ///     <para>
        ///         To use, add the following to DraggableFloat in your xaml: <c>DragEnded="{s:Action InputDragEnded}"</c>
        ///     </para>
        /// </summary>
        public void InputDragEnded(object sender, EventArgs e)
        {
            InputDragging = false;
            ProfileEditorService.UpdateSelectedProfileElement();
        }

        private void LayerPropertyOnUpdated(object? sender, EventArgs e)
        {
            UpdateInputValue();
        }

        private void LayerPropertyOnDataBindingChange(object? sender, LayerPropertyEventArgs<T> e)
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
        /// <summary>
        ///     For internal use only, implement <see cref="PropertyInputViewModel{T}" /> instead.
        /// </summary>
        protected PropertyInputViewModel()
        {
        }

        /// <summary>
        ///     For internal use only, implement <see cref="PropertyInputViewModel{T}" /> instead.
        /// </summary>
        protected PropertyInputViewModel(IModelValidator validator) : base(validator)
        {
        }

        /// <summary>
        ///     Prevents this type being implemented directly, implement <see cref="PropertyInputViewModel{T}" /> instead.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        internal abstract object InternalGuard { get; }

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
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}