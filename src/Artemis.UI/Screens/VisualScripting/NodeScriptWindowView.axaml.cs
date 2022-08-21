using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptWindowView : ReactiveCoreWindow<NodeScriptWindowViewModel>
{
    public NodeScriptWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}