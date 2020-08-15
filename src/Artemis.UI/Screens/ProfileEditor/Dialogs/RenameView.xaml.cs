using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    /// <summary>
    ///     Interaction logic for RenameView.xaml
    /// </summary>
    public partial class RenameView : UserControl
    {
        public RenameView()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            Keyboard.Focus(textBox);
            textBox.SelectAll();
        }
    }
}