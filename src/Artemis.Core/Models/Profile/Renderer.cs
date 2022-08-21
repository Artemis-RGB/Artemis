using System;
using SkiaSharp;

namespace Artemis.Core;

internal class Renderer : IDisposable
{
    private bool _disposed;
    private SKRect _lastBounds;
    private GRContext? _lastGraphicsContext;
    private SKRect _lastParentBounds;
    private bool _valid;
    public SKSurface? Surface { get; private set; }
    public SKPaint? Paint { get; private set; }
    public SKPath? Path { get; private set; }
    public SKPoint TargetLocation { get; private set; }

    public bool IsOpen { get; private set; }

    /// <summary>
    ///     Opens the render context using the dimensions of the provided path
    /// </summary>
    public void Open(SKPath path, Folder? parent)
    {
        if (_disposed)
            throw new ObjectDisposedException("Renderer");

        if (IsOpen)
            throw new ArtemisCoreException("Cannot open render context because it is already open");

        if (path.Bounds != _lastBounds || (parent != null && parent.Bounds != _lastParentBounds) || _lastGraphicsContext != Constants.ManagedGraphicsContext?.GraphicsContext)
            Invalidate();

        if (!_valid || Surface == null)
        {
            SKRect pathBounds = path.Bounds;
            int width = (int) pathBounds.Width;
            int height = (int) pathBounds.Height;

            SKImageInfo imageInfo = new(width, height);
            if (Constants.ManagedGraphicsContext?.GraphicsContext == null)
                Surface = SKSurface.Create(imageInfo);
            else
                Surface = SKSurface.Create(Constants.ManagedGraphicsContext.GraphicsContext, true, imageInfo);

            Path = new SKPath(path);
            Path.Transform(SKMatrix.CreateTranslation(pathBounds.Left * -1, pathBounds.Top * -1));

            TargetLocation = new SKPoint(pathBounds.Location.X, pathBounds.Location.Y);
            if (parent != null)
                TargetLocation -= parent.Bounds.Location;

            Surface.Canvas.ClipPath(Path);

            _lastParentBounds = parent?.Bounds ?? new SKRect();
            _lastBounds = path.Bounds;
            _lastGraphicsContext = Constants.ManagedGraphicsContext?.GraphicsContext;
            _valid = true;
        }

        Paint = new SKPaint();

        Surface.Canvas.Clear();
        Surface.Canvas.Save();

        IsOpen = true;
    }

    public void Close()
    {
        if (_disposed)
            throw new ObjectDisposedException("Renderer");

        Surface?.Canvas.Restore();

        // Looks like every part of the paint needs to be disposed :(
        Paint?.ColorFilter?.Dispose();
        Paint?.ImageFilter?.Dispose();
        Paint?.MaskFilter?.Dispose();
        Paint?.PathEffect?.Dispose();
        Paint?.Dispose();

        Paint = null;

        IsOpen = false;
    }

    public void Invalidate()
    {
        if (_disposed)
            throw new ObjectDisposedException("Renderer");

        _valid = false;
    }

    ~Renderer()
    {
        if (IsOpen)
            Close();
    }

    public void Dispose()
    {
        if (IsOpen)
            Close();

        Surface?.Dispose();
        Paint?.Dispose();
        Path?.Dispose();

        Surface = null;
        Paint = null;
        Path = null;

        _disposed = true;
    }
}