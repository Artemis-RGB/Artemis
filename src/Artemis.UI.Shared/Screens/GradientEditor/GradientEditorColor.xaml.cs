using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Annotations;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    /// <summary>
    ///     Interaction logic for GradientEditorColor.xaml
    /// </summary>
    public partial class GradientEditorColor : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ColorGradientColorProperty = DependencyProperty.Register(nameof(GradientColor), typeof(ColorGradientColor), typeof(GradientEditorColor),
            new FrameworkPropertyMetadata(default(ColorGradientColor), ColorGradientColorPropertyChangedCallback));

        private bool _inCallback;

        public GradientEditorColor()
        {
            InitializeComponent();
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

            // Get the parent canvas
            DependencyObject parent = null;
            DependencyObject current = self;
            while (current != null && !(parent is Canvas))
            {
                parent = VisualTreeHelper.GetParent(current);
                current = parent;
            }

            if (parent is Canvas canvas)
                self.ColorStop.SetValue(Canvas.LeftProperty, (double) 435f * self.GradientColor.Position);

            self.ColorStop.Fill = new SolidColorBrush(Color.FromArgb(
                self.GradientColor.Color.Alpha,
                self.GradientColor.Color.Red,
                self.GradientColor.Color.Green,
                self.GradientColor.Color.Blue
            ));

            self._inCallback = false;
        }
    }
}