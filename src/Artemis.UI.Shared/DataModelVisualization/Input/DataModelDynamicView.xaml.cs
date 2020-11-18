using System.Windows;
using System.Windows.Controls;

namespace Artemis.UI.Shared.Input
{
    /// <summary>
    ///     Interaction logic for DataModelDynamicView.xaml
    /// </summary>
    public partial class DataModelDynamicView : UserControl
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelDynamicView" /> class
        /// </summary>
        public DataModelDynamicView()
        {
            InitializeComponent();
        }

        private void PropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            // DataContext is not set when using left button, I don't know why but there it is
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}