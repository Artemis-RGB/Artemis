using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.UI.Exceptions;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public abstract class PropertyInputViewModel : PropertyChangedBase
    {
        protected PropertyInputViewModel(IProfileEditorService profileEditorService)
        {
            ProfileEditorService = profileEditorService;
        }

        protected IProfileEditorService ProfileEditorService { get; set; }
        public abstract List<Type> CompatibleTypes { get; }

        public bool Initialized { get; private set; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; private set; }
        public bool InputFieldEnabled { get; set; }

        protected object InputValue
        {
            get => LayerPropertyViewModel.LayerProperty.GetCurrentValue();
            set => UpdateInputValue(value);
        }

        public void Initialize(LayerPropertyViewModel layerPropertyViewModel)
        {
            var type = layerPropertyViewModel.LayerProperty.Type;
            if (type.IsEnum)
                type = typeof(Enum);
            if (Initialized)
                throw new ArtemisUIException("Cannot initialize the same property input VM twice");
            if (!CompatibleTypes.Contains(type))
                throw new ArtemisUIException($"This input VM does not support the provided type {type.Name}");

            LayerPropertyViewModel = layerPropertyViewModel;
            layerPropertyViewModel.LayerProperty.ValueChanged += (sender, args) => Update();
            Update();

            Initialized = true;

            OnInitialized();
        }

        public abstract void Update();

        public abstract void ApplyInputDrag(object startValue, double dragDistance);

        protected virtual void OnInitialized()
        {
        }

        private void UpdateInputValue(object value)
        {
            LayerPropertyViewModel.LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            // Force the keyframe engine to update, the edited keyframe might affect the current keyframe progress
            LayerPropertyViewModel.LayerProperty.KeyframeEngine?.Update(0);

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        #region Mouse-based mutations

        private Point _mouseDragStartPoint;
        private object _startValue;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startValue = InputValue;
            ((IInputElement) sender).CaptureMouse();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputMouseUp(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((IInputElement) sender);
            if (position == _mouseDragStartPoint)
                InputFieldEnabled = true;

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition((IInputElement) sender);
                ApplyInputDrag(_startValue, position.X - _mouseDragStartPoint.X);
            }
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (InputFieldEnabled)
            {
                ((UIElement) sender).Focus();
                if (sender is TextBox textBox)
                    textBox.SelectAll();
            }
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputLostFocus(object sender, RoutedEventArgs e)
        {
            InputFieldEnabled = false;
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                InputFieldEnabled = false;
            else if (e.Key == Key.Escape)
            {
                if (sender is TextBox textBox)
                    textBox.Text = _startValue.ToString();
                InputFieldEnabled = false;
            }
        }

        #endregion
    }
}