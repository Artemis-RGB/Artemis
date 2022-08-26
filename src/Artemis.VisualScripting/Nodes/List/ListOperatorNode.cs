using System.Collections;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.List.Screens;

namespace Artemis.VisualScripting.Nodes.List;

[Node("List Operator (Simple)", "Checks if any/all/no value in the input list matches the input value", "List", InputType = typeof(IEnumerable), OutputType = typeof(bool))]
public class ListOperatorNode : Node<ListOperator, ListOperatorNodeCustomViewModel>
{
    public ListOperatorNode() : base("List Operator", "Checks if any/all/no value in the input list matches the input value")
    {
        InputList = CreateInputPin<IList>();
        InputValue = CreateInputPin<object>();

        Ouput = CreateOutputPin<bool>();
    }

    public InputPin<IList> InputList { get; }
    public InputPin<object> InputValue { get; }
    public OutputPin<bool> Ouput { get; }
    
    /// <inheritdoc />
    public override void Evaluate()
    {
        if (InputList.Value == null)
        {
            Ouput.Value = Storage == ListOperator.None;
            return;
        }

        object? input = InputValue.Value;
        if (Storage == ListOperator.Any)
            Ouput.Value = InputList.Value.Cast<object>().Any(v => v.Equals(input));
        else if (Storage == ListOperator.All)
            Ouput.Value = InputList.Value.Cast<object>().All(v => v.Equals(input));
        else if (Storage == ListOperator.All)
            Ouput.Value = InputList.Value.Cast<object>().All(v => !v.Equals(input));
    }
}

public enum ListOperator
{
    Any,
    All,
    None
}