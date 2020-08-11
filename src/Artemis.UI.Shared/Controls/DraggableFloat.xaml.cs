using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Shared.Controls
{
    /// <summary>
    ///     Interaction logic for DraggableFloat.xaml
    /// </summary>
    public partial class DraggableFloat : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(float), typeof(DraggableFloat),
            new FrameworkPropertyMetadata(default(float), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FloatPropertyChangedCallback));

        public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(float), typeof(DraggableFloat));

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

            Value = (float) UltimateRoundingFunction(startValue + stepSize * (x - startX), stepSize, 0.5m);
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
            var seperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var regex = new Regex("^[" + seperator + "][-|0-9]+$|^-?[0-9]*[" + seperator + "]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private static decimal UltimateRoundingFunction(decimal amountToRound, decimal nearstOf, decimal fairness)
        {
            return Math.Floor(amountToRound / nearstOf + fairness) * nearstOf;
        }
    }
}