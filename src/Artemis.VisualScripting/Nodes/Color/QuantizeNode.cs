using Artemis.Core;
using Artemis.Core.Services;
using Artemis.VisualScripting.Nodes.Color.Screens;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

public class QuantizeNodeStorage
{
    public int PaletteSize { get; set; } = 32;
    public bool IgnoreLimits { get; set; }
};

[Node("Quantize", "Quantizes the image into key-colors", "Image", InputType = typeof(SKBitmap), OutputType = typeof(SKColor))]
public class QuantizeNode : Node<QuantizeNodeStorage, QuantizeNodeCustomViewModel>
{
    #region Properties & Fields

    public InputPin<SKBitmap> Image { get; set; }

    public OutputPin<SKColor> Vibrant { get; set; }
    public OutputPin<SKColor> Muted { get; set; }
    public OutputPin<SKColor> DarkVibrant { get; set; }
    public OutputPin<SKColor> DarkMuted { get; set; }
    public OutputPin<SKColor> LightVibrant { get; set; }
    public OutputPin<SKColor> LightMuted { get; set; }

    #endregion

    #region Constructors

    public QuantizeNode()
        : base("Quantize", "Quantizes the image into key-colors")
    {
        Image = CreateInputPin<SKBitmap>("Image");

        Vibrant = CreateOutputPin<SKColor>("Vibrant");
        Muted = CreateOutputPin<SKColor>("Muted");
        DarkVibrant = CreateOutputPin<SKColor>("DarkVibrant");
        DarkMuted = CreateOutputPin<SKColor>("DarkMuted");
        LightVibrant = CreateOutputPin<SKColor>("LightVibrant");
        LightMuted = CreateOutputPin<SKColor>("LightMuted");

        Storage = new QuantizeNodeStorage();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        SKBitmap? image = Image.Value;
        if (image == null) return;

        SKColor[] colorPalette = ColorQuantizer.Quantize(image.Pixels, Storage?.PaletteSize ?? 32);
        ColorSwatch swatch = ColorQuantizer.FindAllColorVariations(colorPalette, Storage?.IgnoreLimits ?? false);

        Vibrant.Value = swatch.Vibrant;
        Muted.Value = swatch.Muted;
        DarkVibrant.Value = swatch.DarkVibrant;
        DarkMuted.Value = swatch.DarkMuted;
        LightVibrant.Value = swatch.LightVibrant;
        LightMuted.Value = swatch.LightMuted;
    }

    #endregion
}