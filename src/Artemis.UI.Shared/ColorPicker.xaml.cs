using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Color), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyChangedCallback));

        public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register(nameof(PopupOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PopupOpenPropertyChangedCallback));


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

        internal byte ColorOpacity
        {
            get => (byte) GetValue(ColorOpacityProperty);
            set => SetValue(ColorOpacityProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void ColorPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;

            colorPicker.SetCurrentValue(ColorOpacityProperty, ((Color) e.NewValue).A);
            colorPicker.OnPropertyChanged(nameof(Color));

            colorPicker._inCallback = false;
        }

        private static void PopupOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;
            colorPicker.OnPropertyChanged(nameof(PopupOpen));
            colorPicker._inCallback = false;
        }

        private static void ColorOpacityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker) d;
            if (colorPicker._inCallback)
                return;

            colorPicker._inCallback = true;

            var color = colorPicker.Color;
            if (e.NewValue is byte opacity)
                color = Color.FromArgb(opacity, color.R, color.G, color.B);
            colorPicker.SetCurrentValue(ColorProperty, color);
            colorPicker.OnPropertyChanged(nameof(ColorOpacity));

            colorPicker._inCallback = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupOpen = !PopupOpen;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}