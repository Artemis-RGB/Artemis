using System;
using System.Diagnostics.CodeAnalysis;
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
        [AllowNull]
        private T _inputValue = default!;

        /// <summary>
        ///     Creates a new instance of the <see cref="PropertyInputViewModel" /> class
        /// </summary>
        /// <param name="layerProperty">The layer property this view model will edit</param>
        /// <param name="profileEditorService">The profile editor service</param>
        protected PropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService)
        {
            LayerProperty = layerProperty;
            ProfileEditorService = profileEditorService;
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
        [AllowNull]
        public T InputValue
        {
            get => _inputValue;
            set
            {
                SetAndNotify(ref _inputValue, value);
                ApplyInputValue();
            }
        }

        internal override object InternalGuard { get; } = new();

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            LayerProperty.Updated += LayerPropertyOnUpdated;
            LayerProperty.CurrentValueSet += LayerPropertyOnUpdated;
            LayerProperty.DataBindingEnabled += LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled += LayerPropertyOnDataBindingChange;
            UpdateInputValue();
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            LayerProperty.Updated -= LayerPropertyOnUpdated;
            LayerProperty.CurrentValueSet -= LayerPropertyOnUpdated;
            LayerProperty.DataBindingEnabled -= LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled -= LayerPropertyOnDataBindingChange;
            base.OnClose();
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
            {
                OnInputValueChanged();
                LayerProperty.SetCurrentValue(_inputValue, ProfileEditorService.CurrentTime);
                OnInputValueApplied();

                if (InputDragging)
                    ProfileEditorService.UpdateProfilePreview();
                else
                    ProfileEditorService.SaveSelectedProfileElement();
            }
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
            ProfileEditorService.SaveSelectedProfileElement();
        }

        private void LayerPropertyOnUpdated(object? sender, EventArgs e)
        {
            UpdateInputValue();
        }

        private void LayerPropertyOnDataBindingChange(object? sender, LayerPropertyEventArgs e)
        {
            OnDataBindingsChanged();
        }

        #endregion
    }

    /// <summary>
    ///     For internal use only, implement <see cref="PropertyInputViewModel{T}" /> instead.
    /// </summary>
    public abstract class PropertyInputViewModel : Screen
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
    }
}