using System;
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
        private bool _disposed;

        #region Constructors

        internal SKTexture(int width, int height, float renderScale)
            : base(width, height, 4, new AverageByteSampler())
        {
            Bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgb888x));
            RenderScale = renderScale;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Color GetColor(in ReadOnlySpan<byte> pixel)
        {
            return new(pixel[0], pixel[1], pixel[2]);
        }

        #endregion

        #region Properties & Fields

        /// <summary>
        ///     Gets the SKBitmap backing this texture
        /// </summary>
        public SKBitmap Bitmap { get; }

        /// <summary>
        ///     Gets the color data in RGB format
        /// </summary>
        protected override ReadOnlySpan<byte> Data => _disposed ? new ReadOnlySpan<byte>() : Bitmap.GetPixelSpan();

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
            _disposed = true;
            Bitmap.Dispose();
        }

        #endregion
    }
}