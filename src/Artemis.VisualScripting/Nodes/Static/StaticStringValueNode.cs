using Artemis.Core;
using Artemis.VisualScripting.Nodes.Static.Screens;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Text-Value", "Outputs a configurable static text value.", "Static", OutputType = typeof(string))]
public class StaticStringValueNode : Node<string, StaticStringValueNodeCustomViewModel>
{
    #region Constructors

    public StaticStringValueNode()
    {
        Name = "Text";
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