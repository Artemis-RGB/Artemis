using System;
using System.Buffers;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    public sealed class SKTexture : ITexture
    {
        #region Constants

        private const int STACK_ALLOC_LIMIT = 1024;

        #endregion

        #region Properties & Fields

        private readonly SKBitmap _bitmap;
        private readonly int _stride;

        public SKBitmap Bitmap => _bitmap;

        public Size Size { get; }

        public ISampler<byte> Sampler { get; set; } = new ArtemisSampler();

        public Color this[in Point point]
        {
            get
            {
                int x = (Size.Width * point.X.Clamp(0, 1)).RoundToInt();
                int y = (Size.Height * point.Y.Clamp(0, 1)).RoundToInt();
                return _bitmap.GetPixel(x, y).ToRgbColor();
            }
        }

        public Color this[in Rectangle rectangle]
        {
            get
            {
                int x = (Size.Width * rectangle.Location.X.Clamp(0, 1)).RoundToInt();
                int y = (Size.Height * rectangle.Location.Y.Clamp(0, 1)).RoundToInt();
                int width = (Size.Width * rectangle.Size.Width.Clamp(0, 1)).RoundToInt();
                int height = (Size.Height * rectangle.Size.Height.Clamp(0, 1)).RoundToInt();

                int bufferSize = width * height * 4;
                if (bufferSize <= STACK_ALLOC_LIMIT)
                {
                    Span<byte> buffer = stackalloc byte[bufferSize];
                    GetRegionData(x, y, width, height, buffer);
                    return Sampler.SampleColor(new SamplerInfo<byte>(width, height, buffer));
                }
                else
                {
                    byte[] rent = ArrayPool<byte>.Shared.Rent(bufferSize);
                    Span<byte> buffer = new Span<byte>(rent).Slice(0, bufferSize);
                    GetRegionData(x, y, width, height, buffer);
                    Color color = Sampler.SampleColor(new SamplerInfo<byte>(width, height, buffer));
                    ArrayPool<byte>.Shared.Return(rent);

                    return color;
                }
            }
        }

        #endregion

        #region Constructors

        public SKTexture(SKBitmap bitmap)
        {
            this._bitmap = bitmap;

            Size = new Size(bitmap.Width, bitmap.Height);
            _stride = bitmap.Width;
        }

        #endregion

        #region Methods

        private void GetRegionData(int x, int y, int width, int height, in Span<byte> buffer)
        {
            int width4 = width * 4;
            ReadOnlySpan<byte> data = _bitmap.GetPixelSpan();
            for (int i = 0; i < height; i++)
            {
                ReadOnlySpan<byte> dataSlice = data.Slice((((y + i) * _stride) + x) * 4, width4);
                Span<byte> destination = buffer.Slice(i * width4, width4);
                dataSlice.CopyTo(destination);
            }
        }

        #endregion
    }
}
