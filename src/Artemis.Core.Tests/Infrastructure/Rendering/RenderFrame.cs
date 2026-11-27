using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     The result of rendering a single frame: the raw canvas and the colors that ended up on each LED.
/// </summary>
public sealed class RenderFrame : IDisposable
{
    internal RenderFrame(TimeSpan time, SKBitmap canvas, IReadOnlyDictionary<ArtemisLed, SKColor> ledColors)
    {
        Time = time;
        Canvas = canvas;
        LedColors = ledColors;
    }

    /// <summary>
    ///     Gets the profile time at which this frame was rendered
    /// </summary>
    public TimeSpan Time { get; }

    /// <summary>
    ///     Gets the raw canvas the profile rendered to, at the harness' render scale
    /// </summary>
    public SKBitmap Canvas { get; }

    /// <summary>
    ///     Gets the color of each LED as sampled from the canvas, this is what the hardware would show
    /// </summary>
    public IReadOnlyDictionary<ArtemisLed, SKColor> LedColors { get; }

    /// <summary>
    ///     Gets the color of the given LED on the given device
    /// </summary>
    public SKColor GetColor(ArtemisDevice device, LedId ledId)
    {
        ArtemisLed led = device.GetLed(ledId, false) ?? throw new ArgumentException($"Device {device} has no LED {ledId}", nameof(ledId));
        return LedColors[led];
    }

    /// <summary>
    ///     Writes canvas.png, leds.png and leds.json to <c>render-output/&lt;test namespace&gt;/&lt;test class&gt;/&lt;name&gt;</c>
    ///     in the test project folder and attaches them to the test result.
    /// </summary>
    /// <returns>The folder the files were written to</returns>
    public string Save(string name)
    {
        using RenderOutputWriter writer = RenderOutput.Begin(name);
        writer.WritePng("canvas.png", Canvas);
        using (SKBitmap leds = RenderOutput.DrawLeds(this))
            writer.WritePng("leds.png", leds);
        writer.WriteJson("leds.json", RenderOutput.ToJson(this));
        return writer.Directory;
    }

    /// <summary>
    ///     Creates a copy that remains usable after this frame is disposed
    /// </summary>
    internal RenderFrame Copy()
    {
        return new RenderFrame(Time, Canvas.Copy(), new Dictionary<ArtemisLed, SKColor>(LedColors));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Canvas.Dispose();
    }
}
