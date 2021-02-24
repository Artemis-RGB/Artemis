using System;
using RGB.NET.Core;
using RGB.NET.Presets.Textures.Sampler;
using SkiaSharp;

namespace Artemis.Core
{
    public sealed class SKTexture : PixelTexture<byte>
    {
        #region Properties & Fields

        private readonly SKBitmap _bitmap;
        public SKBitmap Bitmap => _bitmap;

        protected override ReadOnlySpan<byte> Data => _bitmap.GetPixelSpan();

        #endregion

        #region Constructors

        public SKTexture(SKBitmap bitmap)
            : base(bitmap.Width, bitmap.Height, 4, new AverageByteSampler())
        {
            this._bitmap = bitmap;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Color GetColor(ReadOnlySpan<byte> pixel) => new(pixel[0], pixel[1], pixel[2]);

        #endregion
    }
}
