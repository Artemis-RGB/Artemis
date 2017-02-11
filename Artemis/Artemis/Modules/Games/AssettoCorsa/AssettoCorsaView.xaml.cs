using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.AssettoCorsa
{
    public partial class AssettoCorsaView : UserControl
    {
        public AssettoCorsaView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}