﻿using Artemis.Core;
using Artemis.VisualScripting.Nodes.Operators.Screens;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Enum Equals", "Determines the equality between an input and a selected enum value", "Operators", InputType = typeof(Enum), OutputType = typeof(bool))]
public class EnumEqualsNode : Node<long, EnumEqualsNodeCustomViewModel>
{
    public EnumEqualsNode()
    {
        InputPin = CreateInputPin<Enum>();
        OutputPin = CreateOutputPin<bool>();
    }

    public InputPin<Enum> InputPin { get; }
    public OutputPin<bool> OutputPin { get; }

    /// <inheritdoc />
    public override void Evaluate()
    {
        if (InputPin.Value == null)
            OutputPin.Value = false;
        else
            OutputPin.Value = Convert.ToInt64(InputPin.Value) == Storage;
    }
}