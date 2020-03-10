using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Annotations;
using MaterialDesignExtensions.Controls;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    /// <summary>
    ///     Interaction logic for GradientEditor.xaml
    /// </summary>
    public partial class GradientEditor : MaterialWindow, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorGradientProperty = DependencyProperty.Register(nameof(ColorGradient), typeof(ColorGradient), typeof(GradientEditor),
            new FrameworkPropertyMetadata(default(ColorGradient), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorGradientPropertyChangedCallback));

        public static readonly RoutedEvent ColorGradientChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ColorGradient),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ColorGradient>),
                typeof(GradientEditor));

        private bool _inCallback;

        public GradientEditor(ColorGradient colorGradient)
        {
            DataContext = this;

            InitializeComponent();
            ColorGradient = colorGradient;
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
            var editor = (GradientEditor) d;
            if (editor._inCallback)
                return;

            editor._inCallback = true;
            editor.OnPropertyChanged(nameof(ColorGradient));
            editor._inCallback = false;
        }
    }
}