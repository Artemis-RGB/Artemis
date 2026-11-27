using Artemis.Core.LayerBrushes;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

/// <summary>
///     A per-LED brush that lets the test decide the color of each LED.
/// </summary>
public class PerLedTestBrush : PerLedLayerBrush<PerLedTestBrushProperties>
{
    /// <summary>
    ///     Gets or sets the function that picks the color of an LED, receives the LED and its point relative to the layer
    /// </summary>
    public Func<ArtemisLed, SKPoint, SKColor> ColorSelector { get; set; } = (_, _) => SKColors.White;

    public override void EnableLayerBrush()
    {
    }

    public override void DisableLayerBrush()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
    {
        return ColorSelector(led, renderPoint);
    }
}
