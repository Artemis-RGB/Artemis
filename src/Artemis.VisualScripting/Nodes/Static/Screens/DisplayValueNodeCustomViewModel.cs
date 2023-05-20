using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Shared.VisualScripting;
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
            System.Timers.Timer updateTimer = new(TimeSpan.FromMilliseconds(25.0 / 1000));
            updateTimer.Elapsed += (_, _) => Update();
            updateTimer.Start();
            updateTimer.DisposeWith(d);
        });
    }

    public object? CurrentValue
    {
        get => _currentValue;
        private set => this.RaiseAndSetIfChanged(ref _currentValue, value);
    }

    private void Update()
    {
        try
        {
            CurrentValue = _node.Input.Value;
        }
        catch (Exception ex)
        {
            // Don't crash the timer on exceptions and display the messages as a bit of a nice to have
            CurrentValue = ex.Message;
        }
    }
}