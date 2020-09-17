using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for DraggableFloat.xaml
    /// </summary>
    public partial class DraggableFloat : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(float), typeof(DraggableFloat),
            new FrameworkPropertyMetadata(default(float), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FloatPropertyChangedCallback));

        public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(float), typeof(DraggableFloat));
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min), typeof(float?), typeof(DraggableFloat));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(float?), typeof(DraggableFloat));

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Value),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<float>),
                typeof(DraggableFloat));

        private bool _calledDragStarted;

        private bool _inCallback;
        private Point _mouseDragStartPoint;
        private float _startValue;

        public DraggableFloat()
        {
            InitializeComponent();
        }

        public float Value
        {
            get => (float) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public float StepSize
        {
            get => (float) GetValue(StepSizeProperty);
            set => SetValue(StepSizeProperty, value);
        }

        public float? Min
        {
            get => (float?) GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        public float? Max
        {
            get => (float?) GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler DragStarted;
        public event EventHandler DragEnded;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDragStarted()
        {
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDragEnded()
        {
            DragEnded?.Invoke(this, EventArgs.Empty);
        }

        private static void FloatPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var draggableFloat = (DraggableFloat) d;
            if (draggableFloat._inCallback)
                return;

            draggableFloat._inCallback = true;
            draggableFloat.OnPropertyChanged(nameof(Value));
            draggableFloat._inCallback = false;
        }

        private void InputMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            _startValue = Value;
            ((IInputElement) sender).CaptureMouse();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
        }

        private void InputMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var position = e.GetPosition((IInputElement) sender);
            if (position == _mouseDragStartPoint)
                DisplayInput();
            else
            {
                OnDragEnded();
                _calledDragStarted = false;
            }

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        private void InputMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            e.Handled = true;

            if (!_calledDragStarted)
            {
                OnDragStarted();
                _calledDragStarted = true;
            }

            // Use decimals for everything to avoid floating point errors
            var startValue = new decimal(_startValue);
            var startX = new decimal(_mouseDragStartPoint.X);
            var x = new decimal(e.GetPosition((IInputElement) sender).X);
            var stepSize = new decimal(StepSize);
            if (stepSize == 0)
                stepSize = 0.1m;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                stepSize = stepSize * 10;

            var value = (float) RoundToNearestOf(startValue + stepSize * (x - startX), stepSize);
            if (Min != null)
                value = Math.Max(value, Min.Value);
            if (Max != null)
                value = Math.Min(value, Max.Value);

            Value = value;
        }

        private void InputLostFocus(object sender, RoutedEventArgs e)
        {
            DisplayDragHandle();
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DisplayDragHandle();
            else if (e.Key == Key.Escape)
            {
                DraggableFloatInputTextBox.Text = _startValue.ToString();
                DisplayDragHandle();
            }
        }

        private void DisplayInput()
        {
            DragHandle.Visibility = Visibility.Collapsed;
            DraggableFloatInputTextBox.Visibility = Visibility.Visible;
            DraggableFloatInputTextBox.Focus();
            DraggableFloatInputTextBox.SelectAll();
        }

        private void DisplayDragHandle()
        {
            DraggableFloatInputTextBox.Visibility = Visibility.Collapsed;
            DragHandle.Visibility = Visibility.Visible;
        }

        private void Input_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void Input_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidateInput(sender, e);
        }

        private void Input_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string) e.DataObject.GetData(typeof(string));
                if (!float.TryParse(text, out _))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        // Borrowed from https://stackoverflow.com/a/48082972/5015269 because a regex approach has bad compatibility with
        // different locales
        private bool ValidateInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return false;

            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // If parsing is successful, set Handled to false
            return float.TryParse(fullText, out _);
        }

        /// <summary>
        ///     Rounds the provided decimal to the nearest value of x with a given threshold
        ///     Source: https://stackoverflow.com/a/25922075/5015269
        /// </summary>
        /// <param name="input">The value to round</param>
        /// <param name="nearestOf">The value to round down towards</param>
        private static decimal RoundToNearestOf(decimal input, decimal nearestOf)
        {
            return Math.Floor(input / nearestOf + 0.5m) * nearestOf;
        }
    }
}