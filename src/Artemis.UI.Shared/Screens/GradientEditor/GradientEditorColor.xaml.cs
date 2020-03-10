using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Annotations;
using Artemis.UI.Shared.Utilities;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    /// <summary>
    ///     Interaction logic for GradientEditorColor.xaml
    /// </summary>
    public partial class GradientEditorColor : UserControl, INotifyPropertyChanged
    {
        private const double CanvasWidth = 435.0;

        public static readonly DependencyProperty ColorGradientColorProperty = DependencyProperty.Register(nameof(GradientColor), typeof(ColorGradientColor), typeof(GradientEditorColor),
            new FrameworkPropertyMetadata(default(ColorGradientColor), ColorGradientColorPropertyChangedCallback));

        public static readonly DependencyProperty ColorGradientProperty = DependencyProperty.Register(nameof(ColorGradient), typeof(ColorGradient), typeof(GradientEditorColor),
            new FrameworkPropertyMetadata(default(ColorGradient)));

        private readonly Canvas _previewCanvas;

        private bool _inCallback;
        private double _mouseDownOffset;
        private DateTime _mouseDownTime;

        public GradientEditorColor()
        {
            InitializeComponent();

            var window = Application.Current.Windows.OfType<GradientEditor>().FirstOrDefault();
            _previewCanvas = UIUtilities.FindChild<Canvas>(window, "PreviewCanvas");
        }

        public ColorGradient ColorGradient
        {
            get => (ColorGradient) GetValue(ColorGradientProperty);
            set => SetValue(ColorGradientProperty, value);
        }

        public ColorGradientColor GradientColor
        {
            get => (ColorGradientColor) GetValue(ColorGradientColorProperty);
            set => SetValue(ColorGradientColorProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void ColorGradientColorPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GradientEditorColor) d;
            if (self._inCallback)
                return;

            self._inCallback = true;


            self.OnPropertyChanged(nameof(GradientColor));
            self.Update();
            self._inCallback = false;
        }

        private void Update()
        {
            ColorStop.SetValue(Canvas.LeftProperty, CanvasWidth * GradientColor.Position);
            ColorStop.Fill = new SolidColorBrush(Color.FromArgb(
                GradientColor.Color.Alpha,
                GradientColor.Color.Red,
                GradientColor.Color.Green,
                GradientColor.Color.Blue
            ));
        }

        private void ColorStop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            _mouseDownOffset = (double) ColorStop.GetValue(Canvas.LeftProperty) - e.GetPosition(_previewCanvas).X;
            _mouseDownTime = DateTime.Now;
        }

        private void ColorStop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // On regular click, select this color stop
            if (DateTime.Now - _mouseDownTime <= TimeSpan.FromMilliseconds(250))
            {
            }

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        private void ColorStop_MouseMove(object sender, MouseEventArgs e)
        {
            if (!((IInputElement) sender).IsMouseCaptured)
                return;

            var position = e.GetPosition(_previewCanvas);
            // Position ranges from 0.0 to 1.0
            var newPosition = Math.Min(1, Math.Max(0, Math.Round((position.X + _mouseDownOffset) / CanvasWidth, 3, MidpointRounding.AwayFromZero)));
            GradientColor.Position = (float) newPosition;
            ColorGradient.OnColorValuesUpdated();
            Update();
        }
    }
}