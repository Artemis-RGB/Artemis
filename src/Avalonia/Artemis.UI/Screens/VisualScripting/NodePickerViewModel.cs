using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Avalonia;

namespace Artemis.UI.Screens.VisualScripting;

public class NodePickerViewModel : ActivatableViewModelBase
{
    private readonly INodeService _nodeService;

    private bool _isVisible;
    private Point _position;

    public bool IsVisible
    {
        get => _isVisible;
        set => RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public Point Position
    {
        get => _position;
        set => RaiseAndSetIfChanged(ref _position, value);
    }

    public NodePickerViewModel(INodeService nodeService)
    {
        _nodeService = nodeService;
    }

    public void Show(Point position)
    {
        IsVisible = true;
        Position = position;
    }

    public void Hide()
    {
        IsVisible = false;
    }
}