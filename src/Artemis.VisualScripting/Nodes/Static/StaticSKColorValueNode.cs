using Artemis.Core;
using Artemis.VisualScripting.Nodes.Static.Screens;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Color-Value", "Outputs a configurable color value.", "Static", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
public class StaticSKColorValueNode : Node<SKColor, StaticSKColorValueNodeCustomViewModel>
{
    #region Constructors

    public StaticSKColorValueNode()
        : base("Color", "Outputs a configurable color value.")
    {
        Output = CreateOutputPin<SKColor>();
        Storage = new SKColor(255, 0, 0);
    }

    #endregion

    #region Properties & Fields

    public OutputPin<SKColor> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Storage;
    }

    #endregion
}