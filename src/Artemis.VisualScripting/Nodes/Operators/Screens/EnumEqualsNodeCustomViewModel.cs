using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Controls.Mixins;
using DynamicData;
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
                    EnumValues.AddRange(Enum.GetValues(_node.InputPin.ConnectedTo.First().Type).Cast<Enum>());

                this.RaisePropertyChanged(nameof(CurrentValue));
            }

            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.InputPin.PinConnected += x, x => _node.InputPin.PinConnected -= x)
                .Subscribe(p => EnumValues.AddRange(Enum.GetValues(p.EventArgs.Value.Type).Cast<Enum>()))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.InputPin.PinDisconnected += x, x => _node.InputPin.PinDisconnected -= x)
                .Subscribe(_ => EnumValues.Clear())
                .DisposeWith(d);
        });
    }

    public ObservableCollection<Enum> EnumValues { get; } = new();

    public int CurrentValue
    {
        get => _node.Storage;
        set
        {
            if (!Equals(_node.Storage, value))
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<int>(_node, value));
        }
    }
}