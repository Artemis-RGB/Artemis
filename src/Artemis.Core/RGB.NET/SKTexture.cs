using System;
using Artemis.Core.SkiaSharp;
using RGB.NET.Core;
using RGB.NET.Presets.Textures.Sampler;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a SkiaSharp-based RGB.NET PixelTexture
    /// </summary>
    public sealed class SKTexture : PixelTexture<byte>, IDisposable
    {
        private SKPixmap? _pixelData;
        private SKImage? _rasterImage;

        #region Constructors

        internal SKTexture(IManagedGraphicsContext? managedGraphicsContext, int width, int height, float renderScale)
            : base(width, height, 4, new AverageByteSampler())
        {
            ImageInfo = new SKImageInfo(width, height);
            if (managedGraphicsContext == null)
                Surface = SKSurface.Create(ImageInfo);
            else
                Surface = SKSurface.Create(managedGraphicsContext.GraphicsContext, true, ImageInfo);
            RenderScale = renderScale;
        }

        #endregion

        #region Methods

        internal void CopyPixelData()
        {
            using SKImage skImage = Surface.Snapshot();

            _rasterImage?.Dispose();
            _pixelData?.Dispose();
            _rasterImage = skImage.ToRasterImage();
            _pixelData = _rasterImage.PeekPixels();
        }

        /// <inheritdoc />
        protected override Color GetColor(in ReadOnlySpan<byte> pixel)
        {
            return new(pixel[2], pixel[1], pixel[0]);
        }

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
        protected override ReadOnlySpan<byte> Data => _pixelData != null ? _pixelData.GetPixelSpan() : ReadOnlySpan<byte>.Empty;

        /// <summary>
        ///     Gets the render scale of the texture
        /// </summary>
        public float RenderScale { get; }

        /// <summary>
        ///     Gets a boolean indicating whether <see cref="Invalidate" /> has been called on this texture, indicating it should
        ///     be replaced
        /// </summary>
        public bool IsInvalid { get; private set; }

        /// <summary>
        ///     Invalidates the texture
        /// </summary>
        public void Invalidate()
        {
            IsInvalid = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Surface.Dispose();
            _pixelData?.Dispose();
        }

        #endregion
    }
}