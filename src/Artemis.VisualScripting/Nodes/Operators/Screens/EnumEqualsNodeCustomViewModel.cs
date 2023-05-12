using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Threading;
using DynamicData;
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

    public ObservableCollection<EnumValueItem> EnumValues { get; } = new();

    public EnumValueItem CurrentValue
    {
        get => EnumValues.FirstOrDefault(v => v.Value == _node.Storage);
        set
        {
            if (!Equals(_node.Storage, value.Value))
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<long>(_node, value.Value));
        }
    }

    private void AddEnumValues(Type type)
    {
        Dispatcher.UIThread.Post(() =>
        {
            List<EnumValueItem> values = Enum.GetValues(type).Cast<Enum>().Select(e => new EnumValueItem(value: Convert.ToInt64(e), name: e.Humanize())).ToList();
            if (values.Count > 20)
                EnumValues.AddRange(values.OrderBy(v => v.Name));
            else
                EnumValues.AddRange(values);

            this.RaisePropertyChanged(nameof(CurrentValue));
        }, DispatcherPriority.Background);
    }
}

/// <summary>
///     Represents a single enum value
/// </summary>
public class EnumValueItem
{
    /// <summary>
    ///     Creates a new instance of the <see cref="EnumValueItem" /> class.
    /// </summary>
    public EnumValueItem(long value, string name)
    {
        Value = value;
        Name = name;
    }

    /// <summary>
    ///     The underlying value of the enum
    /// </summary>
    public long Value { get; set; }
    
    /// <summary>
    ///     The name of the enum value
    /// </summary>
    public string Name { get; set; }
}