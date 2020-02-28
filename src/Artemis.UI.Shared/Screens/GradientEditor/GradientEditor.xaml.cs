using System.Windows;
using Artemis.Core.Models.Profile;
using MaterialDesignExtensions.Controls;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    /// <summary>
    ///     Interaction logic for GradientEditor.xaml
    /// </summary>
    public partial class GradientEditor : MaterialWindow
    {
        public GradientEditor(DependencyProperty colorGradientProperty)
        {
            ColorGradientProperty = colorGradientProperty;
            InitializeComponent();
        }

        public DependencyProperty ColorGradientProperty { get; }

        public ColorGradient ColorGradient
        {
            get => (ColorGradient) GetValue(ColorGradientProperty);
            set => SetValue(ColorGradientProperty, value);
        }
    }
}