using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared.Properties;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for GradientPicker.xaml
    /// </summary>
    public partial class GradientPicker : INotifyPropertyChanged
    {
        private static IColorPickerService? _colorPickerService;
        private bool _inCallback;

        /// <summary>
        ///     Creates a new instance of the <see cref="GradientPicker" /> class
        /// </summary>
        public GradientPicker()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the color gradient
        /// </summary>
        public ColorGradient ColorGradient
        {
            get => (ColorGradient) GetValue(ColorGradientProperty);
            set => SetValue(ColorGradientProperty, value);
        }

        /// <summary>
        ///     Gets or sets the dialog host in which to show the gradient dialog
        /// </summary>
        public string DialogHost
        {
            get => (string) GetValue(DialogHostProperty);
            set => SetValue(DialogHostProperty, value);
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

        /// <summary>
        ///     Occurs when the dialog has opened
        /// </summary>
        public event EventHandler? DialogOpened;

        /// <summary>
        ///     Occurs when the dialog has closed
        /// </summary>
        public event EventHandler? DialogClosed;

        /// <summary>
        ///     Invokes the <see cref="PropertyChanged" /> event
        /// </summary>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Invokes the <see cref="DialogOpened" /> event
        /// </summary>
        protected virtual void OnDialogOpened()
        {
            DialogOpened?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Invokes the <see cref="DialogClosed" /> event
        /// </summary>
        protected virtual void OnDialogClosed()
        {
            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private static void ColorGradientPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GradientPicker gradientPicker = (GradientPicker) d;
            if (gradientPicker._inCallback)
                return;

            gradientPicker._inCallback = true;
            gradientPicker.OnPropertyChanged(nameof(ColorGradient));
            gradientPicker._inCallback = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_colorPickerService == null)
                return;

            Execute.OnUIThread(async () =>
            {
                OnDialogOpened();
                await _colorPickerService.ShowGradientPicker(ColorGradient, DialogHost);
                OnDialogClosed();
            });
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Static WPF fields

        /// <summary>
        ///     Gets or sets the color gradient
        /// </summary>
        public static readonly DependencyProperty ColorGradientProperty = DependencyProperty.Register(nameof(ColorGradient), typeof(ColorGradient), typeof(GradientPicker),
            new FrameworkPropertyMetadata(default(ColorGradient), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorGradientPropertyChangedCallback));

        /// <summary>
        ///     Gets or sets the dialog host in which to show the gradient dialog
        /// </summary>
        public static readonly DependencyProperty DialogHostProperty = DependencyProperty.Register(nameof(DialogHost), typeof(string), typeof(GradientPicker),
            new FrameworkPropertyMetadata(default(string)));

        /// <summary>
        ///     Occurs when the color gradient has changed
        /// </summary>
        public static readonly RoutedEvent ColorGradientChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ColorGradient), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ColorGradient>), typeof(GradientPicker));

        #endregion
    }
}