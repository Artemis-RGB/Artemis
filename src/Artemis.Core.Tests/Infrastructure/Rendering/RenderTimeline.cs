using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     A sequence of frames rendered at increasing timestamps
/// </summary>
public sealed class RenderTimeline : IDisposable
{
    internal RenderTimeline(IReadOnlyList<RenderFrame> frames)
    {
        Frames = frames;
    }

    public IReadOnlyList<RenderFrame> Frames { get; }

    public RenderFrame this[int index] => Frames[index];

    /// <summary>
    ///     Writes canvas-strip.png, leds-strip.png (all frames stacked top to bottom) and timeline.json to
    ///     <c>render-output/&lt;test namespace&gt;/&lt;test class&gt;/&lt;name&gt;</c> in the test project folder and attaches
    ///     them to the test result.
    /// </summary>
    /// <returns>The folder the files were written to</returns>
    public string Save(string name)
    {
        using RenderOutputWriter writer = RenderOutput.Begin(name);
        using (SKBitmap canvasStrip = RenderOutput.DrawStrip(Frames, f => f.Canvas.Copy()))
            writer.WritePng("canvas-strip.png", canvasStrip);
        using (SKBitmap ledStrip = RenderOutput.DrawStrip(Frames, RenderOutput.DrawLeds))
            writer.WritePng("leds-strip.png", ledStrip);
        writer.WriteJson("timeline.json", Frames.Select(RenderOutput.ToJson).ToList());
        return writer.Directory;
    }

    /// <summary>
    ///     Creates a copy that remains usable after this timeline is disposed
    /// </summary>
    internal RenderTimeline Copy()
    {
        return new RenderTimeline(Frames.Select(f => f.Copy()).ToList());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (RenderFrame frame in Frames)
            frame.Dispose();
    }
}
