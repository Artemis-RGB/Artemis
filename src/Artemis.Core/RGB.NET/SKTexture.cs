using System;
using System.Collections.Generic;
using Artemis.Core.SkiaSharp;
using HPPH;
using HPPH.SkiaSharp;
using RGB.NET.Core;
using RGB.NET.Presets.Extensions;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a SkiaSharp-based RGB.NET PixelTexture
/// </summary>
public sealed class SKTexture : ITexture, IDisposable
{
    #region Constructors

    internal SKTexture(IManagedGraphicsContext? graphicsContext, int width, int height, float scale, IReadOnlyCollection<ArtemisDevice> devices)
    {
        RenderScale = scale;
        Size = new Size(width, height);

        ImageInfo = new SKImageInfo(width, height);
        Surface = graphicsContext == null
            ? SKSurface.Create(ImageInfo)
            : SKSurface.Create(graphicsContext.GraphicsContext, true, ImageInfo);

        foreach (ArtemisDevice artemisDevice in devices)
        {
            foreach (ArtemisLed artemisLed in artemisDevice.Leds)
                _ledRects[artemisLed.RgbLed] = CreateSampleRect(artemisLed.RgbLed.AbsoluteBoundary, scale, width, height);
        }
    }

    #endregion

    private static SKRectI CreateSampleRect(Rectangle boundary, float scale, int width, int height)
    {
        (int left, int right) = ScaleSpan(boundary.Location.X, boundary.Size.Width, scale, width);
        (int top, int bottom) = ScaleSpan(boundary.Location.Y, boundary.Size.Height, scale, height);
        return new SKRectI(left, top, right, bottom);
    }

    private static (int Start, int End) ScaleSpan(float location, float size, float scale, int max)
    {
        float start = location * scale;
        float end = (location + size) * scale;
        if (end <= 0 || start >= max)
            return (0, 0);

        // Always sample at least one pixel
        int startPixel = Math.Clamp(start.RoundToInt(), 0, max - 1);
        int endPixel = Math.Clamp(end.RoundToInt(), startPixel + 1, max);
        return (startPixel, endPixel);
    }

    internal Color GetColorAtRenderTarget(in RenderTarget renderTarget)
    {
        if (_image == null) return Color.Transparent;

        SKRectI skRectI = _ledRects[renderTarget.Led];

        if (skRectI.Width <= 0 || skRectI.Height <= 0)
            return Color.Transparent;

        return _image[skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height].Average().ToColor();
    }

    /// <inheritdoc />
    ~SKTexture()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Surface.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Methods

    /// <summary>
    ///     Invalidates the texture
    /// </summary>
    public void Invalidate()
    {
        IsInvalid = true;
    }

    internal void CopyPixelData()
    {
        using SKImage skImage = Surface.Snapshot();
        _image = skImage.ToImage();
    }

    #endregion

    #region Properties & Fields

    private IImage? _image;
    private readonly Dictionary<Led, SKRectI> _ledRects = [];

    /// <inheritdoc />
    public Size Size { get; }
    /// <inheritdoc />
    public Color this[Point point] => Color.Transparent;
    /// <inheritdoc />
    public Color this[Rectangle rectangle] => Color.Transparent;

    /// <summary>
    ///     Gets the SKBitmap backing this texture
    /// </summary>
    public SKSurface Surface { get; }

    /// <summary>
    ///     Gets the image info used to create the <see cref="Surface" />
    /// </summary>
    public SKImageInfo ImageInfo { get; }

    /// <summary>
    ///     Gets the render scale of the texture
    /// </summary>
    public float RenderScale { get; }

    /// <summary>
    ///     Gets a boolean indicating whether <see cref="Invalidate" /> has been called on this texture, indicating it should
    ///     be replaced
    /// </summary>
    public bool IsInvalid { get; private set; }

    #endregion
}