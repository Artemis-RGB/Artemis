using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeViewModel : ActivatableViewModelBase
{
    private Point _position;

    public NodeViewModel(INode node)
    {
        Node = node;
    }

    public INode Node { get; }

    public Point Position
    {
        get => _position;
        set => RaiseAndSetIfChanged(ref _position, value);
    }
}