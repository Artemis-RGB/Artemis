using Artemis.Core;
using Artemis.VisualScripting.Nodes.Static.Screens;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Numeric-Value", "Outputs a configurable static numeric value.", "Static", OutputType = typeof(Numeric))]
public class StaticNumericValueNode : Node<Numeric, StaticNumericValueNodeCustomViewModel>
{
    #region Constructors

    public StaticNumericValueNode()
    {
        Name = "Numeric";
        Output = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Properties & Fields

    public OutputPin<Numeric> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Storage;
    }

    #endregion
}