using System.Windows;
using System.Windows.Controls;

namespace Artemis.UI.Shared

{
    /// <summary>
    ///     Interaction logic for ProfileConfigurationIcon.xaml
    /// </summary>
    public partial class ProfileConfigurationIcon : UserControl
    {
        /// <summary>
        ///     Gets or sets the <see cref="Core.ProfileConfigurationIcon" /> to display
        /// </summary>
        public static readonly DependencyProperty ConfigurationIconProperty =
            DependencyProperty.Register(nameof(ConfigurationIcon), typeof(Core.ProfileConfigurationIcon), typeof(ProfileConfigurationIcon));

        /// <summary>
        ///     Creates a new instance of the <see cref="ProfileConfigurationIcon" /> class
        /// </summary>
        public ProfileConfigurationIcon()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the <see cref="Core.ProfileConfigurationIcon" /> to display
        /// </summary>
        public Core.ProfileConfigurationIcon ConfigurationIcon
        {
            get => (Core.ProfileConfigurationIcon) GetValue(ConfigurationIconProperty);
            set => SetValue(ConfigurationIconProperty, value);
        }
    }
}