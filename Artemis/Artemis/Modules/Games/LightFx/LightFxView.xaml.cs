using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.LightFx
{
    public partial class LightFxView : UserControl
    {
        public LightFxView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}