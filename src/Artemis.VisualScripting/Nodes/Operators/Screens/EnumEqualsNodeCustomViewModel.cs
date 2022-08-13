using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Controls.Mixins;
using Avalonia.Threading;
using DynamicData;
using FluentAvalonia.Core;
using Humanizer;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Operators.Screens;

public class EnumEqualsNodeCustomViewModel : CustomNodeViewModel
{
    private readonly EnumEqualsNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public EnumEqualsNodeCustomViewModel(EnumEqualsNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(CurrentValue));
        this.WhenActivated(d =>
        {
            if (_node.InputPin.ConnectedTo.Any())
            {
                EnumValues.Clear();
                if (_node.InputPin.ConnectedTo.First().Type.IsEnum)
                    AddEnumValues(_node.InputPin.ConnectedTo.First().Type);
            }

            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.InputPin.PinConnected += x, x => _node.InputPin.PinConnected -= x)
                .Subscribe(p => AddEnumValues(p.EventArgs.Value.Type))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.InputPin.PinDisconnected += x, x => _node.InputPin.PinDisconnected -= x)
                .Subscribe(_ => EnumValues.Clear())
                .DisposeWith(d);
        });
    }

    private void AddEnumValues(Type type)
    {
        Dispatcher.UIThread.Post(() =>
        {
            List<(long, string)> values = Enum.GetValues(type).Cast<Enum>().Select(e => (Convert.ToInt64(e), e.Humanize())).ToList();
            if (values.Count > 20)
                EnumValues.AddRange(values.OrderBy(v => v.Item2));
            else
                EnumValues.AddRange(Enum.GetValues(type).Cast<Enum>().Select(e => (Convert.ToInt64(e), e.Humanize())));

            this.RaisePropertyChanged(nameof(CurrentValue));
        }, DispatcherPriority.Background);
    }

    public ObservableCollection<(long, string)> EnumValues { get; } = new();

    public (long, string) CurrentValue
    {
        get => EnumValues.FirstOrDefault(v => v.Item1 == _node.Storage);
        set
        {
            if (!Equals(_node.Storage, value.Item1))
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<long>(_node, value.Item1));
        }
    }
}