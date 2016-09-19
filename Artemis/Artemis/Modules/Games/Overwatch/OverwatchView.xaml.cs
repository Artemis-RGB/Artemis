using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.Overwatch
{
    /// <summary>
    ///     Interaction logic for OverwatchView.xaml
    /// </summary>
    public partial class OverwatchView : UserControl
    {
        public OverwatchView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}