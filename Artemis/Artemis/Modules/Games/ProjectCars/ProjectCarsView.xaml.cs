using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.ProjectCars
{
    public partial class ProjectCarsView : UserControl
    {
        public ProjectCarsView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}