using Artemis.Core;
using Artemis.VisualScripting.Nodes.Static.Screens;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Boolean-Value", "Outputs a configurable static boolean value.", "Static", OutputType = typeof(bool))]
public class StaticBooleanValueNode : Node<bool, StaticBooleanValueNodeCustomViewModel>
{
    #region Constructors

    public StaticBooleanValueNode()
        : base("Boolean", "Outputs a configurable static boolean value.")
    {
        Output = CreateOutputPin<bool>();
    }

    #endregion

    #region Properties & Fields

    public OutputPin<bool> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Storage;
    }

    #endregion
}