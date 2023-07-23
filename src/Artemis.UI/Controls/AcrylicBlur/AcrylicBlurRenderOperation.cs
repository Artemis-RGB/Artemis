using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace Artemis.UI.Controls.AcrylicBlur;

public class AcrylicBlurRenderOperation : ICustomDrawOperation
{
    private static SKShader? _acrylicNoiseShader;

    private readonly AcrylicBlur _acrylicBlur;
    private readonly ImmutableExperimentalAcrylicMaterial _material;
    private readonly int _blur;
    private readonly Rect _bounds;
    private SKImage? _backgroundSnapshot;
    private bool _disposed;

    public AcrylicBlurRenderOperation(AcrylicBlur acrylicBlur, ImmutableExperimentalAcrylicMaterial material, int blur, Rect bounds)
    {
        _acrylicBlur = acrylicBlur;
        _material = material;
        _blur = blur;
        _bounds = bounds;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _backgroundSnapshot?.Dispose();
        _disposed = true;
    }

    public bool HitTest(Point p) => _bounds.Contains(p);

    static SKColorFilter CreateAlphaColorFilter(double opacity)
    {
        if (opacity > 1)
            opacity = 1;
        byte[] c = new byte[256];
        byte[] a = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            c[i] = (byte) i;
            a[i] = (byte) (i * opacity);
        }

        return SKColorFilter.CreateTable(a, c, c, c);
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AcrylicBlurRenderOperation));

        ISkiaSharpApiLeaseFeature? leaseFeature = context.PlatformImpl.GetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
            return;
        using ISkiaSharpApiLease lease = leaseFeature.Lease();

        if (!lease.SkCanvas.TotalMatrix.TryInvert(out SKMatrix currentInvertedTransform) || lease.SkSurface == null)
            return;

        if (lease.SkCanvas.GetLocalClipBounds(out SKRect bounds) && !bounds.Contains(SKRect.Create(bounds.Left, bounds.Top, (float) _acrylicBlur.Bounds.Width, (float) _acrylicBlur.Bounds.Height)))
        {
            Dispatcher.UIThread.Invoke(() => _acrylicBlur.InvalidateVisual());
        }
        else
        {
            _backgroundSnapshot?.Dispose();
            _backgroundSnapshot = lease.SkSurface.Snapshot();
        }
        
        _backgroundSnapshot ??= lease.SkSurface.Snapshot();
        using SKShader? backdropShader = SKShader.CreateImage(_backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);
        using SKSurface? blurred = SKSurface.Create(
            lease.GrContext,
            false,
            new SKImageInfo((int) Math.Ceiling(_bounds.Width), (int) Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul)
        );
        using (SKImageFilter? filter = SKImageFilter.CreateBlur(_blur, _blur, SKShaderTileMode.Clamp))
        using (SKPaint blurPaint = new SKPaint {Shader = backdropShader, ImageFilter = filter})
        {
            blurred.Canvas.DrawRect(0, 0, (float) _bounds.Width, (float) _bounds.Height, blurPaint);

            using (SKImage? blurSnap = blurred.Snapshot())
            using (SKShader? blurSnapShader = SKShader.CreateImage(blurSnap))
            using (SKPaint blurSnapPaint = new SKPaint {Shader = blurSnapShader, IsAntialias = true})
            {
                // Rendering twice to reduce opacity
                lease.SkCanvas.DrawRect(0, 0, (float) _bounds.Width, (float) _bounds.Height, blurSnapPaint);
                lease.SkCanvas.DrawRect(0, 0, (float) _bounds.Width, (float) _bounds.Height, blurSnapPaint);
            }

            //return;
            using SKPaint acrylliPaint = new SKPaint();
            acrylliPaint.IsAntialias = true;

            double opacity = 1;

            const double noiseOpacity = 0.0225;

            Color tintColor = _material.TintColor;
            SKColor tint = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

            if (_acrylicNoiseShader == null)
            {
                using Stream? stream = typeof(SkiaPlatform).Assembly.GetManifestResourceStream("Avalonia.Skia.Assets.NoiseAsset_256X256_PNG.png");
                using SKBitmap? bitmap = SKBitmap.Decode(stream);
                _acrylicNoiseShader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat).WithColorFilter(CreateAlphaColorFilter(noiseOpacity));
            }

            using (SKShader? backdrop = SKShader.CreateColor(new SKColor(_material.MaterialColor.R, _material.MaterialColor.G, _material.MaterialColor.B, _material.MaterialColor.A)))
            using (SKShader? tintShader = SKShader.CreateColor(tint))
            using (SKShader? effectiveTint = SKShader.CreateCompose(backdrop, tintShader))
            using (SKShader? compose = SKShader.CreateCompose(effectiveTint, _acrylicNoiseShader))
            {
                acrylliPaint.Shader = compose;
                acrylliPaint.IsAntialias = true;
                lease.SkCanvas.DrawRect(0, 0, (float) _bounds.Width, (float) _bounds.Height, acrylliPaint);
            }
        }
    }

    public Rect Bounds => _bounds.Inflate(4);

    public bool Equals(ICustomDrawOperation? other)
    {
        return other is AcrylicBlurRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
    }
}