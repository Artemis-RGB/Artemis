using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Average color", "Calculate the average if all colors in the image", "Image", InputType = typeof(SKBitmap), OutputType = typeof(SKColor))]
public class AverageColorNode : Node
{
    #region Properties & Fields

    public InputPin<SKBitmap> Image { get; set; }

    public OutputPin<SKColor> Average { get; set; }

    #endregion

    #region Constructors

    public AverageColorNode()
        : base("Average color", "Calculate the average if all colors in the image")
    {
        Image = CreateInputPin<SKBitmap>();
        Average = CreateOutputPin<SKColor>();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        SKBitmap? image = Image.Value;
        if (image == null) return;

        Span<SKColor> colors = image.Pixels;
        int r = 0, g = 0, b = 0;
        foreach (SKColor color in colors)
        {
            r += color.Red;
            g += color.Green;
            b += color.Blue;
        }

        Average.Value = new SKColor((byte)(r / colors.Length), (byte)(g / colors.Length), (byte)(b / colors.Length));
    }

    #endregion
}