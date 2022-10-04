using System.Collections;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Nodes.List.Screens;

namespace Artemis.VisualScripting.Nodes.List;

[Node("List Operator (Advanced)", "Checks if any/all/no values in the input list match a condition", "List", InputType = typeof(IEnumerable), OutputType = typeof(bool))]
public class ListOperatorPredicateNode : Node<ListOperatorEntity, ListOperatorPredicateNodeCustomViewModel>, IDisposable
{
    private readonly object _scriptLock = new();
    private ListOperatorPredicateStartNode _startNode;

    public ListOperatorPredicateNode()
    {
        Name = "List Operator";
        _startNode = new ListOperatorPredicateStartNode {X = -200};

        InputList = CreateInputPin<IList>();
        Output = CreateOutputPin<bool>();

        InputList.PinConnected += InputListOnPinConnected;
    }

    public InputPin<IList> InputList { get; }
    public OutputPin<bool> Output { get; }
    public NodeScript<bool>? Script { get; private set; }

    public override void Initialize(INodeScript script)
    {
        Storage ??= new ListOperatorEntity();

        lock (_scriptLock)
        {
            Script = Storage?.Script != null
                ? new NodeScript<bool>("Is match", "Determines whether the current list item is a match", Storage.Script, script.Context, new List<DefaultNode> {_startNode})
                : new NodeScript<bool>("Is match", "Determines whether the current list item is a match", script.Context, new List<DefaultNode> {_startNode});
        }
    }

    /// <inheritdoc />
    public override void Evaluate()
    {
        if (Storage == null)
            return;

        if (InputList.Value == null)
        {
            Output.Value = Storage.Operator == ListOperator.None;
            return;
        }

        lock (_scriptLock)
        {
            if (Script == null)
                return;

            if (Storage.Operator == ListOperator.Any)
                Output.Value = InputList.Value.Cast<object>().Any(EvaluateItem);
            else if (Storage.Operator == ListOperator.All)
                Output.Value = InputList.Value.Cast<object>().All(EvaluateItem);
            else if (Storage.Operator == ListOperator.None)
                Output.Value = InputList.Value.Cast<object>().All(v => !EvaluateItem(v));
        }
    }

    private bool EvaluateItem(object item)
    {
        if (Script == null || _startNode == null)
            return false;

        _startNode.Item = item;
        Script.Run();
        return Script.Result;
    }

    private void UpdateStartNode()
    {
        Type? type = InputList.ConnectedTo.FirstOrDefault()?.Type;
        // List must be generic or there's no way to tell what objects it contains in advance, that's not supported for now
        if (type is not {IsGenericType: true})
            return;

        Type listType = type.GetGenericArguments().Single();
        _startNode?.ChangeType(listType);
    }

    private void InputListOnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        lock (_scriptLock)
        {
            UpdateStartNode();
            Script?.LoadConnections();
        }
    }

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        Script?.Dispose();
        Script = null;
        _startNode = null;
    }

    #endregion
}