using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : INotifyPropertyChanged
    {
        private static IColorPickerService? _colorPickerService;

        /// <summary>
        ///     Gets or sets the color
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Color), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyChangedCallback));

        /// <summary>
        ///     Gets or sets a boolean indicating that the popup containing the color picker is open
        /// </summary>
        public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register(nameof(PopupOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PopupOpenPropertyChangedCallback));

        /// <summary>
        ///     Gets or sets a boolean indicating whether the popup should stay open when clicked outside of it
        /// </summary>
        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, StaysOpenPropertyChangedCallback));

        internal static readonly DependencyProperty ColorOpacityProperty = DependencyProperty.Register(nameof(ColorOpacity), typeof(byte), typeof(ColorPicker),
            new FrameworkPropertyMetadata((byte) 255, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorOpacityPropertyChangedCallback));

        /// <summary>
        ///     Occurs when the selected color has changed
        /// </summary>
        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Color),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>),
                typeof(ColorPicker));

        /// <summary>
        ///     Occurs when the popup opens or closes
        /// </summary>
        public static readonly RoutedEvent PopupOpenChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(PopupOpen),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(ColorPicker));

        private bool _inCallback;

        /// <summary>
        ///     Creates a new instance of the <see cref="ColorPicker" /> class
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        ///     Gets or sets the color
        /// </summary>
        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating that the popup containing the color picker is open
        /// </summary>
        public bool PopupOpen
        {
            get => (bool) GetValue(PopupOpenProperty);
            set => SetValue(PopupOpenProperty, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the popup should stay open when clicked outside of it
        /// </summary>
        public bool StaysOpen
        {
            get => (bool) GetValue(StaysOpenProperty);
            set => SetValue(StaysOpenProperty, value);
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

        internal byte ColorOpacity
        {
            get => (byte) GetValue(ColorOpacityProperty);
            set => SetValue(ColorOpacityProperty, value);
        }

        /// <summary>
        ///     Invokes the <see cref="PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
            _colorPickerService?.UpdateColorDisplay(colorPicker.Color);

            colorPicker._inCallback = false;
        }

        private static void PopupOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;
            colorPicker.OnPropertyChanged(nameof(PopupOpen));

            if ((bool) e.NewValue)
                colorPicker.PopupOpened();
            else
                colorPicker.PopupClosed();

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
            _colorPickerService?.UpdateColorDisplay(colorPicker.Color);

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
            if (_colorPickerService == null) return;
            OnDragStarted();

            if (_colorPickerService.PreviewSetting.Value)
                _colorPickerService.StartColorDisplay();
        }

        private void Slider_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_colorPickerService == null) return;
            OnDragEnded();
            _colorPickerService.StopColorDisplay();
        }

        private void PreviewCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (_colorPickerService == null) return;
            _colorPickerService.PreviewSetting.Value = PreviewCheckBox.IsChecked ?? false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_colorPickerService == null) return;
            PreviewCheckBox.IsChecked = _colorPickerService.PreviewSetting.Value;
            _colorPickerService.PreviewSetting.SettingChanged += PreviewSettingOnSettingChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_colorPickerService == null) return;
            _colorPickerService.PreviewSetting.SettingChanged -= PreviewSettingOnSettingChanged;
        }

        private void PreviewSettingOnSettingChanged(object? sender, EventArgs e)
        {
            if (_colorPickerService == null) return;
            PreviewCheckBox.IsChecked = _colorPickerService.PreviewSetting.Value;
        }

        private void PopupClosed()
        {
            _colorPickerService?.QueueRecentColor(Color);
        }

        private void PopupOpened()
        {
            RecentColorsContainer.ItemsSource = new ObservableCollection<Color>(_colorPickerService.RecentColors);
        }

        private void SelectRecentColor(object sender, MouseButtonEventArgs e)
        {
            Color = (Color) ((Rectangle) sender).DataContext;
            PopupOpen = false;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;


        #region Events

        /// <summary>
        ///     Occurs when dragging the color picker has started
        /// </summary>
        public event EventHandler? DragStarted;

        /// <summary>
        ///     Occurs when dragging the color picker has ended
        /// </summary>
        public event EventHandler? DragEnded;

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