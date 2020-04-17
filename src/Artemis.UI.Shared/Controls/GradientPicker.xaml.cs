using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Annotations;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.Controls
{
    /// <summary>
    ///     Interaction logic for GradientPicker.xaml
    /// </summary>
    public partial class GradientPicker : UserControl, INotifyPropertyChanged
    {
        private static IGradientPickerService _gradientPickerService;
        private bool _inCallback;

        public GradientPicker()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Used by the gradient picker to load saved gradients, do not touch or it'll just throw an exception
        /// </summary>
        public static IGradientPickerService GradientPickerService
        {
            private get => _gradientPickerService;
            set
            {
                if (_gradientPickerService != null)
                    throw new AccessViolationException("This is not for you to touch");
                _gradientPickerService = value;
            }
        }

        /// <summary>
        ///     Gets or sets the currently selected color gradient
        /// </summary>
        public ColorGradient ColorGradient
        {
            get => (ColorGradient) GetValue(ColorGradientProperty);
            set => SetValue(ColorGradientProperty, value);
        }

        /// <summary>
        ///     Gets or sets the currently selected color gradient
        /// </summary>
        public string DialogHost
        {
            get => (string) GetValue(DialogHostProperty);
            set => SetValue(DialogHostProperty, value);
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
            GradientPickerService.ShowGradientPicker(ColorGradient, DialogHost);
        }

        #region Static WPF fields

        public static readonly DependencyProperty ColorGradientProperty = DependencyProperty.Register(nameof(ColorGradient), typeof(ColorGradient), typeof(GradientPicker),
            new FrameworkPropertyMetadata(default(ColorGradient), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorGradientPropertyChangedCallback));

        public static readonly DependencyProperty DialogHostProperty = DependencyProperty.Register(nameof(DialogHost), typeof(string), typeof(GradientPicker),
            new FrameworkPropertyMetadata(default(string)));

        public static readonly RoutedEvent ColorGradientChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ColorGradient), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ColorGradient>), typeof(GradientPicker));

        #endregion
    }
}