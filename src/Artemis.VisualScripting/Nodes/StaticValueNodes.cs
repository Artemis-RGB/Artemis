using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes;

[Node("Numeric-Value", "Outputs a configurable static numeric value.", "Static", OutputType = typeof(Numeric))]
public class StaticNumericValueNode : Node<Numeric, StaticNumericValueNodeCustomViewModel>
{
    #region Constructors

    public StaticNumericValueNode()
        : base("Numeric", "Outputs a configurable numeric value.")
    {
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

[Node("String-Value", "Outputs a configurable static string value.", "Static", OutputType = typeof(string))]
public class StaticStringValueNode : Node<string, StaticStringValueNodeCustomViewModel>
{
    #region Constructors

    public StaticStringValueNode()
        : base("String", "Outputs a configurable string value.")
    {
        Output = CreateOutputPin<string>();
    }

    #endregion

    #region Properties & Fields

    public OutputPin<string> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Storage;
    }

    #endregion
}