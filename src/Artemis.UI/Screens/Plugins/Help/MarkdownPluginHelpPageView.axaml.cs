using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Help;

public partial class MarkdownPluginHelpPageView : ReactiveUserControl<MarkdownPluginHelpPageViewModel>
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