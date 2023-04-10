using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.VisualScripting;

public partial class NodeScriptWindowView : ReactiveAppWindow<NodeScriptWindowViewModel>
{
    public NodeScriptWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

}