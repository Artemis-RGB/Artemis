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
        #region Constructors

        internal SKTexture(SKBitmap bitmap)
            : base(bitmap.Width, bitmap.Height, 4, new AverageByteSampler())
        {
            Bitmap = bitmap;
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
        protected override ReadOnlySpan<byte> Data => Bitmap.GetPixelSpan();

        /// <inheritdoc />
        public void Dispose()
        {
            Bitmap.Dispose();
        }

        #endregion
    }
}