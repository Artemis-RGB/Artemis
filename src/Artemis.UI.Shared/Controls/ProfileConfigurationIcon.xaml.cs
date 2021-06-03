using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared

{
    /// <summary>
    ///     Interaction logic for ProfileConfigurationIcon.xaml
    /// </summary>
    public partial class ProfileConfigurationIcon : UserControl
    {
        /// <summary>
        ///     Gets or sets the <see cref="PackIconKind" />
        /// </summary>
        public static readonly DependencyProperty ConfigurationIconProperty =
            DependencyProperty.Register(nameof(ConfigurationIcon), typeof(Core.ProfileConfigurationIcon), typeof(ProfileConfigurationIcon));

        public ProfileConfigurationIcon()
        {
            InitializeComponent();
        }

        public Core.ProfileConfigurationIcon ConfigurationIcon
        {
            get => (Core.ProfileConfigurationIcon) GetValue(ConfigurationIconProperty);
            set => SetValue(ConfigurationIconProperty, value);
        }
    }
}