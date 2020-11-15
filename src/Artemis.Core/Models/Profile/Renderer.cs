using System;
using SkiaSharp;

namespace Artemis.Core
{
    internal class Renderer : IDisposable
    {
        private bool _valid;
        private bool _disposed;
        public SKBitmap? Bitmap { get; private set; }
        public SKCanvas? Canvas { get; private set; }
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

            if (!_valid)
            {
                SKRect pathBounds = path.Bounds;
                int width = (int) pathBounds.Width;
                int height = (int) pathBounds.Height;

                Bitmap = new SKBitmap(width, height);
                Path = new SKPath(path);
                Canvas = new SKCanvas(Bitmap);
                Path.Transform(SKMatrix.MakeTranslation(pathBounds.Left * -1, pathBounds.Top * -1));

                TargetLocation = new SKPoint(pathBounds.Location.X, pathBounds.Location.Y);
                if (parent != null)
                    TargetLocation -= parent.Bounds.Location;

                Canvas.ClipPath(Path);

                _valid = true;
            }

            Paint = new SKPaint();

            Canvas.Clear();
            Canvas.Save();

            IsOpen = true;
        }

        public void Close()
        {
            if (_disposed)
                throw new ObjectDisposedException("Renderer");
            
            Canvas.Restore();
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

        public void Dispose()
        {
            if (IsOpen)
                Close();

            Canvas?.Dispose();
            Paint?.Dispose();
            Path?.Dispose();
            Bitmap?.Dispose();

            Canvas = null;
            Paint = null;
            Path = null;
            Bitmap = null;

            _disposed = true;
        }
    }
}