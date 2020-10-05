using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        private static IColorPickerService _colorPickerService;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Color), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyChangedCallback));

        public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register(nameof(PopupOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PopupOpenPropertyChangedCallback));

        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, StaysOpenPropertyChangedCallback));

        internal static readonly DependencyProperty ColorOpacityProperty = DependencyProperty.Register(nameof(ColorOpacity), typeof(byte), typeof(ColorPicker),
            new FrameworkPropertyMetadata((byte) 255, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorOpacityPropertyChangedCallback));

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Color),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>),
                typeof(ColorPicker));

        public static readonly RoutedEvent PopupOpenChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(PopupOpen),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(ColorPicker));

        private bool _inCallback;

        public ColorPicker()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        ///     Used by the gradient picker to load saved gradients, do not touch or it'll just throw an exception
        /// </summary>
        internal static IColorPickerService ColorPickerService
        {
            set
            {
                if (_colorPickerService != null)
                    throw new AccessViolationException("This is not for you to touch");
                _colorPickerService = value;
            }
        }

        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public bool PopupOpen
        {
            get => (bool) GetValue(PopupOpenProperty);
            set => SetValue(PopupOpenProperty, value);
        }

        public bool StaysOpen
        {
            get => (bool) GetValue(StaysOpenProperty);
            set => SetValue(StaysOpenProperty, value);
        }

        internal byte ColorOpacity
        {
            get => (byte) GetValue(ColorOpacityProperty);
            set => SetValue(ColorOpacityProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void ColorPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;

            colorPicker.SetCurrentValue(ColorOpacityProperty, ((Color) e.NewValue).A);
            colorPicker.OnPropertyChanged(nameof(Color));
            _colorPickerService.UpdateColorDisplay(colorPicker.Color);

            colorPicker._inCallback = false;
        }

        private static void PopupOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;
            colorPicker.OnPropertyChanged(nameof(PopupOpen));
            colorPicker._inCallback = false;
        }

        private static void StaysOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;
            colorPicker.OnPropertyChanged(nameof(PopupOpen));
            colorPicker._inCallback = false;
        }

        private static void ColorOpacityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;

            Color color = colorPicker.Color;
            if (e.NewValue is byte opacity)
                color = Color.FromArgb(opacity, color.R, color.G, color.B);
            colorPicker.SetCurrentValue(ColorProperty, color);
            colorPicker.OnPropertyChanged(nameof(ColorOpacity));
            _colorPickerService.UpdateColorDisplay(colorPicker.Color);

            colorPicker._inCallback = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupOpen = !PopupOpen;
            e.Handled = true;
        }

        private void ColorGradient_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Slider_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnDragStarted();

            if (_colorPickerService.PreviewSetting.Value)
                _colorPickerService.StartColorDisplay();
        }

        private void Slider_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            OnDragEnded();
            _colorPickerService.StopColorDisplay();
        }

        private void PreviewCheckBoxClick(object sender, RoutedEventArgs e)
        {
            _colorPickerService.PreviewSetting.Value = PreviewCheckBox.IsChecked.Value;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PreviewCheckBox.IsChecked = _colorPickerService.PreviewSetting.Value;
            _colorPickerService.PreviewSetting.SettingChanged += PreviewSettingOnSettingChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _colorPickerService.PreviewSetting.SettingChanged -= PreviewSettingOnSettingChanged;
        }

        private void PreviewSettingOnSettingChanged(object sender, EventArgs e)
        {
            PreviewCheckBox.IsChecked = _colorPickerService.PreviewSetting.Value;
        }


        #region Events

        public event EventHandler DragStarted;
        public event EventHandler DragEnded;

        protected virtual void OnDragStarted()
        {
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDragEnded()
        {
            DragEnded?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}