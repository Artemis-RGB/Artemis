using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    /// <summary>
    ///     Interaction logic for CounterStrikeView.xaml
    /// </summary>
    public partial class EurotruckSimulator2View : UserControl
    {
        public EurotruckSimulator2View()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}