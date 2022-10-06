using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Invert Color", "Inverts a color by a specified amount in percent", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
public class InvertSKColorNode : Node
{
    public InvertSKColorNode()
    {
        Input = CreateInputPin<SKColor>();
        Output = CreateOutputPin<SKColor>();
    }

    public InputPin<SKColor> Input { get; }
    public OutputPin<SKColor> Output { get; set; }

    public override void Evaluate()
    {
        Output.Value = new SKColor(
            (byte) (255 - Input.Value.Red),
            (byte) (255 - Input.Value.Green),
            (byte) (255 - Input.Value.Blue),
            Input.Value.Alpha
        );
    }
}