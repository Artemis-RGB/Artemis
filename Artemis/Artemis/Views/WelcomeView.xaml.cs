using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Views
{
    /// <summary>
    ///     Interaction logic for WelcomeView.xaml
    /// </summary>
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://github.com/SpoinkyNL/Artemis/wiki/Frequently-Asked-Questions-(FAQ)");
        }
    }
}