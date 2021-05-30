using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Screens.Sidebar
{
    /// <summary>
    ///     Interaction logic for SidebarCategoryView.xaml
    /// </summary>
    public partial class SidebarCategoryView : UserControl
    {
        public SidebarCategoryView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}