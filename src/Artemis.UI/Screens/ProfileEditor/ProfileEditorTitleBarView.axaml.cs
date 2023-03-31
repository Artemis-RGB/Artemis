using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class ProfileEditorTitleBarView : UserControl
{
    public ProfileEditorTitleBarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}