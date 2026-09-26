using System;
using System.Collections.Generic;
using Artemis.Core.SkiaSharp;
using HPPH;
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
        _readback = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));

        foreach (ArtemisDevice artemisDevice in devices)
        {
            foreach (ArtemisLed artemisLed in artemisDevice.Leds)
            {
                _ledRects[artemisLed.RgbLed] = SKRectI.Create(
                    (int)(artemisLed.AbsoluteRectangle.Left * RenderScale),
                    (int)(artemisLed.AbsoluteRectangle.Top * RenderScale),
                    (int)(artemisLed.AbsoluteRectangle.Width * RenderScale),
                    (int)(artemisLed.AbsoluteRectangle.Height * RenderScale)
                );
            }
        }
    }

    #endregion

    internal Color GetColorAtRenderTarget(in RenderTarget renderTarget)
    {
        if (!_hasPixels) return Color.Transparent;

        SKRectI skRectI = _ledRects[renderTarget.Led];

        if (skRectI.Width <= 0 || skRectI.Height <= 0)
            return Color.Transparent;

        return AveragePixels(_readback, skRectI);
    }

    internal static unsafe Color AveragePixels(SKBitmap bitmap, SKRectI rectangle)
    {
        int left = Math.Max(0, rectangle.Left);
        int top = Math.Max(0, rectangle.Top);
        int right = Math.Min(bitmap.Width, rectangle.Right);
        int bottom = Math.Min(bitmap.Height, rectangle.Bottom);
        if (left >= right || top >= bottom)
            return Color.Transparent;

        byte* pixels = (byte*)bitmap.GetPixels().ToPointer();
        if (pixels == null)
            return Color.Transparent;

        long blue = 0;
        long green = 0;
        long red = 0;
        long alpha = 0;
        int rowBytes = bitmap.RowBytes;
        for (int y = top; y < bottom; y++)
        {
            byte* pixel = pixels + y * rowBytes + left * 4;
            for (int x = left; x < right; x++, pixel += 4)
            {
                blue += pixel[0];
                green += pixel[1];
                red += pixel[2];
                alpha += pixel[3];
            }
        }

        float count = (right - left) * (bottom - top);
        return new ColorBGRA(
            (byte)MathF.Round(blue / count),
            (byte)MathF.Round(green / count),
            (byte)MathF.Round(red / count),
            (byte)MathF.Round(alpha / count)).ToColor();
    }

    /// <inheritdoc />
    ~SKTexture()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _readback.Dispose();
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
        _hasPixels = Surface.ReadPixels(_readback.Info, _readback.GetPixels(), _readback.RowBytes, 0, 0);
    }

    #endregion

    #region Properties & Fields

    private readonly SKBitmap _readback;
    private bool _hasPixels;
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
