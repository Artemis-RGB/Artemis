using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Screens.Module.ProfileEditor.Dialogs
{
    /// <summary>
    ///     Interaction logic for ProfileElementRenameView.xaml
    /// </summary>
    public partial class ProfileElementRenameView : UserControl
    {
        public ProfileElementRenameView()
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