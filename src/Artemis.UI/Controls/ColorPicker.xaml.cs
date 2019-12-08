using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Artemis.UI.Controls
{
    /// <summary>
    ///     Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Color), OnColorPropertyChanged)
        );

        private byte _colorOpacity;

        private static void OnColorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ColorPicker colorPicker)
            {
                colorPicker.OnPropertyChanged(nameof(Color));
                colorPicker.OnPropertyChanged(nameof(ColorCode));
                colorPicker.OnPropertyChanged(nameof(SolidColor));
                colorPicker.OnPropertyChanged(nameof(ColorOpacity));
            }
        }

        public ColorPicker()
        {
            InitializeComponent();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Color))
            {
                if (Color.A != _colorOpacity) 
                    Color = Color.FromArgb(_colorOpacity, Color.R, Color.G, Color.B);
            }
        }

        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public string ColorCode
        {
            get => Color.ToString();
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                        Color = new Color();
                    else
                    {
                        var color = ColorConverter.ConvertFromString(value);
                        if (color is Color c)
                        {
                            _colorOpacity = c.A;
                            Color = c;
                        }
                    }
                }
                catch (FormatException)
                {
                    // ignored
                }
            }
        }

        public Color? SolidColor => Color.FromRgb(Color.R, Color.G, Color.B);

        public byte ColorOpacity
        {
            get => _colorOpacity;
            set
            {
                _colorOpacity = value;
                if (Color.A != _colorOpacity)
                {
                    Color = Color.FromArgb(_colorOpacity, Color.R, Color.G, Color.B);
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public bool PopupOpen { get; set; }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupOpen = !PopupOpen;
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}