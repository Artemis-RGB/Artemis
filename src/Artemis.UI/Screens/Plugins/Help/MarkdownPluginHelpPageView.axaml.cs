using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Plugins.Help;

public partial class MarkdownPluginHelpPageView : UserControl
{
    public MarkdownPluginHelpPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}