using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class DisplayValueNodeCustomViewModel : CustomNodeViewModel
{
    private readonly DisplayValueNode _node;
    private object? _currentValue;

    public DisplayValueNodeCustomViewModel(DisplayValueNode node, INodeScript script) : base(node, script)
    {
        _node = node;

        // Because the DisplayValueNode has no output it never evaluates, manually do so here
        this.WhenActivated(d =>
        {
            DispatcherTimer updateTimer = new(TimeSpan.FromMilliseconds(25.0 / 1000), DispatcherPriority.Normal, Update);
            updateTimer.Start();
            Disposable.Create(() => updateTimer.Stop()).DisposeWith(d);
        });
    }

    public object? CurrentValue
    {
        get => _currentValue;
        private set => this.RaiseAndSetIfChanged(ref _currentValue, value);
    }

    private void Update(object? sender, EventArgs e)
    {
        CurrentValue = _node.Input.Value;
    }
}