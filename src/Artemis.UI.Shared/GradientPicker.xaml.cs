using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Annotations;
using Artemis.UI.Shared.Screens.GradientEditor;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for GradientPicker.xaml
    /// </summary>
    public partial class GradientPicker : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorGradientProperty = DependencyProperty.Register(nameof(ColorGradient), typeof(ColorGradient), typeof(GradientPicker),
            new FrameworkPropertyMetadata(default(ColorGradient), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorGradientPropertyChangedCallback));

        public static readonly RoutedEvent ColorGradientChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ColorGradient),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ColorGradient>),
                typeof(GradientPicker));

        private bool _inCallback;

        public GradientPicker()
        {
            InitializeComponent();
        }

        public ColorGradient ColorGradient
        {
            get => (ColorGradient) GetValue(ColorGradientProperty);
            set => SetValue(ColorGradientProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void ColorGradientPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gradientPicker = (GradientPicker) d;
            if (gradientPicker._inCallback)
                return;

            gradientPicker._inCallback = true;
            gradientPicker.OnPropertyChanged(nameof(ColorGradient));
            gradientPicker._inCallback = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var gradientEditor = new GradientEditor(ColorGradient);
            gradientEditor.ShowDialog();
        }
    }
}