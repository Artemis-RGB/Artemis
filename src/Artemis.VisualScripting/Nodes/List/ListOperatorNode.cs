using System.Collections;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.List.Screens;

namespace Artemis.VisualScripting.Nodes.List;

[Node("List Operator (Simple)", "Checks if any/all/no values in the input list match the input value", "List", InputType = typeof(IEnumerable), OutputType = typeof(bool))]
public class ListOperatorNode : Node<ListOperator, ListOperatorNodeCustomViewModel>
{
    public ListOperatorNode()
    {
        InputList = CreateInputPin<IList>();
        InputValue = CreateInputPin<object>();

        Output = CreateOutputPin<bool>();
    }

    public InputPin<IList> InputList { get; }
    public InputPin<object> InputValue { get; }
    public OutputPin<bool> Output { get; }
    
    /// <inheritdoc />
    public override void Evaluate()
    {
        if (InputList.Value == null)
        {
            Output.Value = Storage == ListOperator.None;
            return;
        }

        object? input = InputValue.Value;
        if (Storage == ListOperator.Any)
            Output.Value = InputList.Value.Cast<object>().Any(v => v.Equals(input));
        else if (Storage == ListOperator.All)
            Output.Value = InputList.Value.Cast<object>().All(v => v.Equals(input));
        else if (Storage == ListOperator.None)
            Output.Value = InputList.Value.Cast<object>().All(v => !v.Equals(input));
    }
}

public enum ListOperator
{
    Any,
    All,
    None
}