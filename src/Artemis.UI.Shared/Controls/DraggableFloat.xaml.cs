using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for DraggableFloat.xaml
    /// </summary>
    public partial class DraggableFloat : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the current value
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(float), typeof(DraggableFloat),
            new FrameworkPropertyMetadata(default(float), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FloatPropertyChangedCallback));

        /// <summary>
        ///     Gets or sets the step size when dragging
        /// </summary>
        public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(float), typeof(DraggableFloat));

        /// <summary>
        ///     Gets or sets the minimum value
        /// </summary>
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min), typeof(float?), typeof(DraggableFloat));

        /// <summary>
        ///     Gets or sets the maximum value
        /// </summary>
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(float?), typeof(DraggableFloat));

        /// <summary>
        ///     Occurs when the value has changed
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Value),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<float>),
                typeof(DraggableFloat));

        private readonly Regex _inputRegex = new("^[.][-|0-9]+$|^-?[0-9]*[.]{0,1}[0-9]*$");

        private bool _calledDragStarted;

        private bool _inCallback;
        private Point _mouseDragStartPoint;
        private float _startValue;

        /// <summary>
        ///     Creates a new instance of the <see cref="DraggableFloat" /> class
        /// </summary>
        public DraggableFloat()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the current value
        /// </summary>
        public float Value
        {
            get => (float) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        ///     Gets or sets the current value as a string
        /// </summary>
        public string InputValue
        {
            get => Value.ToString("N3", CultureInfo.InvariantCulture);
            set => UpdateValue(value);
        }

        /// <summary>
        ///     Gets or sets the step size when dragging
        /// </summary>
        public float StepSize
        {
            get => (float) GetValue(StepSizeProperty);
            set => SetValue(StepSizeProperty, value);
        }

        /// <summary>
        ///     Gets or sets the minimum value
        /// </summary>
        public float? Min
        {
            get => (float?) GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        /// <summary>
        ///     Gets or sets the maximum value
        /// </summary>
        public float? Max
        {
            get => (float?) GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        private void UpdateValue(string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedResult))
                return;

            Value = parsedResult;
            OnPropertyChanged(nameof(InputValue));
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

        #region Event handlers

        private static void FloatPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DraggableFloat draggableFloat = (DraggableFloat) d;
            if (draggableFloat._inCallback)
                return;

            draggableFloat._inCallback = true;
            draggableFloat.OnPropertyChanged(nameof(Value));
            draggableFloat.OnPropertyChanged(nameof(InputValue));
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

            Point position = e.GetPosition((IInputElement) sender);
            if (position == _mouseDragStartPoint)
            {
                DisplayInput();
            }
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
            decimal startValue = new(_startValue);
            decimal startX = new(_mouseDragStartPoint.X);
            decimal x = new(e.GetPosition((IInputElement) sender).X);
            decimal stepSize = new(StepSize);
            if (stepSize == 0)
                stepSize = 0.1m;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                stepSize = stepSize * 10;

            float value = (float) RoundToNearestOf(startValue + stepSize * (x - startX), stepSize);
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
            {
                DisplayDragHandle();
            }
            else if (e.Key == Key.Escape)
            {
                DraggableFloatInputTextBox.Text = _startValue.ToString(CultureInfo.InvariantCulture);
                DisplayDragHandle();
            }
        }

        private void Input_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void Input_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_inputRegex.IsMatch(e.Text);
        }

        private void Input_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (e.DataObject.GetData(typeof(string)) is string text && !_inputRegex.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Occurs when dragging has started
        /// </summary>
        public event EventHandler? DragStarted;

        /// <summary>
        ///     Occurs when dragging has ended
        /// </summary>
        public event EventHandler? DragEnded;

        /// <summary>
        ///     Invokes the <see cref="PropertyChanged" /> event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Invokes the <see cref="DragStarted" /> event
        /// </summary>
        protected virtual void OnDragStarted()
        {
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Invokes the <see cref="DragEnded" /> event
        /// </summary>
        protected virtual void OnDragEnded()
        {
            DragEnded?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}