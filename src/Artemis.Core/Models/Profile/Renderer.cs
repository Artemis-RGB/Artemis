using System;
using SkiaSharp;

namespace Artemis.Core
{
    internal class Renderer : IDisposable
    {
        private bool _disposed;
        public SKBitmap? Bitmap { get; private set; }
        public SKCanvas? Canvas { get; private set; }
        public SKPaint? Paint { get; private set; }
        public SKPath? Path { get; private set; }
        public SKPath? ClipPath { get; private set; }
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

            int width = (int) path.Bounds.Width;
            int height = (int) path.Bounds.Height;

            if (Bitmap == null)
                Bitmap = new SKBitmap(width, height);
            else if (Bitmap.Info.Width != width || Bitmap.Info.Height != height)
                Bitmap = new SKBitmap(width, height);

            Path = new SKPath(path);
            Canvas = new SKCanvas(Bitmap);
            Paint = new SKPaint();

            Path.Transform(SKMatrix.MakeTranslation(Path.Bounds.Left * -1, Path.Bounds.Top * -1));
            Canvas.Clear();

            TargetLocation = new SKPoint(path.Bounds.Location.X, path.Bounds.Location.Y);
            if (parent != null)
                TargetLocation -= parent.Path.Bounds.Location;

            ClipPath = new SKPath(Path);
            ClipPath.Transform(SKMatrix.MakeTranslation(TargetLocation.X, TargetLocation.Y));

            IsOpen = true;
        }

        public void Close()
        {
            if (_disposed)
                throw new ObjectDisposedException("Renderer");

            Canvas?.Dispose();
            Paint?.Dispose();
            Path?.Dispose();
            ClipPath?.Dispose();

            Canvas = null;
            Paint = null;
            Path = null;
            ClipPath = null;

            IsOpen = false;
        }

        public void Dispose()
        {
            if (IsOpen)
                Close();

            Bitmap?.Dispose();
            Bitmap = null;

            _disposed = true;
        }
    }
}