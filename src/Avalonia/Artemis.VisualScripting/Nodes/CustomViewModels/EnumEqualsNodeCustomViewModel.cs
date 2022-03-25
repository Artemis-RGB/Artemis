using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared.VisualScripting;
using DynamicData;

namespace Artemis.VisualScripting.Nodes.CustomViewModels;

public class EnumEqualsNodeCustomViewModel : CustomNodeViewModel
{
    private readonly EnumEqualsNode _node;

    public EnumEqualsNodeCustomViewModel(EnumEqualsNode node, INodeScript script) : base(node, script)
    {
        _node = node;
    }

    public ObservableCollection<(Enum, string)> EnumValues { get; } = new();

    // public override void OnActivate()
    // {
    //     _node.InputPin.PinConnected += InputPinOnPinConnected;
    //     _node.InputPin.PinDisconnected += InputPinOnPinDisconnected;
    //
    //     if (_node.InputPin.Value != null && _node.InputPin.Value.GetType().IsEnum)
    //         EnumValues.AddRange(EnumUtilities.GetAllValuesAndDescriptions(_node.InputPin.Value.GetType()));
    //     base.OnActivate();
    // }
    //
    // public override void OnDeactivate()
    // {
    //     _node.InputPin.PinConnected -= InputPinOnPinConnected;
    //     _node.InputPin.PinDisconnected -= InputPinOnPinDisconnected;
    //
    //     base.OnDeactivate();
    // }

    private void InputPinOnPinDisconnected(object sender, SingleValueEventArgs<IPin> e)
    {
        EnumValues.Clear();
    }

    private void InputPinOnPinConnected(object sender, SingleValueEventArgs<IPin> e)
    {
        EnumValues.Clear();
        if (_node.InputPin.Value != null && _node.InputPin.Value.GetType().IsEnum)
            EnumValues.AddRange(EnumUtilities.GetAllValuesAndDescriptions(_node.InputPin.Value.GetType()));
    }
}