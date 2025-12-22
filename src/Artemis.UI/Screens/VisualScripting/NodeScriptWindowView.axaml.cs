using Artemis.UI.Shared;
using Avalonia;

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