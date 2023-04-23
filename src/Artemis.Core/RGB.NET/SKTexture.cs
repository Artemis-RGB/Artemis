using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Artemis.Core.SkiaSharp;
using RGB.NET.Core;
using RGB.NET.Presets.Textures.Sampler;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a SkiaSharp-based RGB.NET PixelTexture
/// </summary>
public sealed class SKTexture : PixelTexture<byte>, IDisposable
{
    private readonly Dictionary<Led, SKRectI> _ledRects;
    private readonly SKPixmap _pixelData;
    private readonly IntPtr _pixelDataPtr;

    #region Constructors

    internal SKTexture(IManagedGraphicsContext? graphicsContext, int width, int height, float scale, IReadOnlyCollection<ArtemisDevice> devices) : base(width, height, DATA_PER_PIXEL,
        new AverageByteSampler())
    {
        ImageInfo = new SKImageInfo(width, height);
        Surface = graphicsContext == null
            ? SKSurface.Create(ImageInfo)
            : SKSurface.Create(graphicsContext.GraphicsContext, true, ImageInfo);
        RenderScale = scale;
        _pixelDataPtr = Marshal.AllocHGlobal(ImageInfo.BytesSize);
        _pixelData = new SKPixmap(ImageInfo, _pixelDataPtr, ImageInfo.RowBytes);

        _ledRects = new Dictionary<Led, SKRectI>();
        foreach (ArtemisDevice artemisDevice in devices)
        {
            foreach (ArtemisLed artemisLed in artemisDevice.Leds)
            {
                _ledRects[artemisLed.RgbLed] = SKRectI.Create(
                    (int) (artemisLed.AbsoluteRectangle.Left * RenderScale),
                    (int) (artemisLed.AbsoluteRectangle.Top * RenderScale),
                    (int) (artemisLed.AbsoluteRectangle.Width * RenderScale),
                    (int) (artemisLed.AbsoluteRectangle.Height * RenderScale)
                );
            }
        }
    }

    #endregion

    internal Color GetColorAtRenderTarget(in RenderTarget renderTarget)
    {
        if (Data.Length == 0) return Color.Transparent;
        SKRectI skRectI = _ledRects[renderTarget.Led];

        if (skRectI.Width <= 0 || skRectI.Height <= 0)
            return Color.Transparent;

        SamplerInfo<byte> samplerInfo = new(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height, Stride, DataPerPixel, Data);

        Span<byte> pixelData = stackalloc byte[DATA_PER_PIXEL];
        Sampler.Sample(samplerInfo, pixelData);

        return GetColor(pixelData);
    }

    private void ReleaseUnmanagedResources()
    {
        Marshal.FreeHGlobal(_pixelDataPtr);
    }

    /// <inheritdoc />
    ~SKTexture()
    {
        ReleaseUnmanagedResources();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Surface.Dispose();
        _pixelData.Dispose();

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    #region Constants

    private const int DATA_PER_PIXEL = 4;

    #endregion

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
        skImage.ReadPixels(_pixelData);
    }

    /// <inheritdoc />
    protected override Color GetColor(in ReadOnlySpan<byte> pixel)
    {
        return new Color(pixel[2], pixel[1], pixel[0]);
    }

    /// <inheritdoc />
    public override Color this[in Rectangle rectangle] => Color.Transparent;

    #endregion

    #region Properties & Fields

    /// <summary>
    ///     Gets the SKBitmap backing this texture
    /// </summary>
    public SKSurface Surface { get; }

    /// <summary>
    ///     Gets the image info used to create the <see cref="Surface" />
    /// </summary>
    public SKImageInfo ImageInfo { get; }

    /// <summary>
    ///     Gets the color data in RGB format
    /// </summary>
    protected override ReadOnlySpan<byte> Data => _pixelData.GetPixelSpan();

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