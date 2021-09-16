using System;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Enum Equals", "Determines the equality between an input and a selected enum value", InputType = typeof(Enum), OutputType = typeof(bool))]
    public class EnumEqualsNode : Node<EnumEqualsNodeCustomViewModel>
    {
        public EnumEqualsNode() : base("Enum Equals", "Determines the equality between an input and a selected enum value")
        {
            InputPin = CreateInputPin<Enum>();
            OutputPin = CreateOutputPin<bool>();
        }

        public InputPin<Enum> InputPin { get; }
        public OutputPin<bool> OutputPin { get; }

        #region Overrides of Node

        /// <inheritdoc />
        public override void Evaluate()
        {
            OutputPin.Value = InputPin.Value != null && InputPin.Value.Equals(Storage);
        }

        #endregion
    }
}