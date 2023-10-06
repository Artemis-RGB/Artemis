using SkiaSharp;

namespace Artemis.Core.Services.Core;

/// <summary>
///     Represents a renderer that renders to a canvas.
/// </summary>
public interface IRenderer
{
    /// <summary>
    ///     Renders to the provided canvas, the delta is the time in seconds since the last time <see cref="Render" /> was
    ///     called.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="delta">The time in seconds since the last time <see cref="Render" /> was called.</param>
    void Render(SKCanvas canvas, double delta);

    /// <summary>
    ///     Called after the rendering has taken place.
    /// </summary>
    /// <param name="texture">The texture that the render resulted in.</param>
    void PostRender(SKTexture texture);
}