using System.Windows.Controls;
using System.Windows.Navigation;

namespace Artemis.Modules.Games.FormulaOne2017
{
    public partial class FormulaOne2017View : UserControl
    {
        public FormulaOne2017View()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}