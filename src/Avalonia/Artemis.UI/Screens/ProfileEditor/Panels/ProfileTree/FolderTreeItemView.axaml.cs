using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public partial class FolderTreeItemView : UserControl
    {
        public FolderTreeItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
