using System;
using System.Buffers;
using System.Runtime.InteropServices;
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
        private readonly bool _isScaledDown;
        private readonly SKPixmap _pixelData;
        private readonly IntPtr _pixelDataPtr;

        #region Constructors

        internal SKTexture(IManagedGraphicsContext? graphicsContext, int width, int height, float scale) : base(width, height, DATA_PER_PIXEL, new AverageByteSampler())
        {
            ImageInfo = new SKImageInfo(width, height);
            Surface = graphicsContext == null
                ? SKSurface.Create(ImageInfo)
                : SKSurface.Create(graphicsContext.GraphicsContext, true, ImageInfo);
            RenderScale = scale;
            _isScaledDown = Math.Abs(scale - 1) > 0.001;
            _pixelDataPtr = Marshal.AllocHGlobal(ImageInfo.BytesSize);
            _pixelData = new SKPixmap(ImageInfo, _pixelDataPtr, ImageInfo.RowBytes);
        }

        #endregion

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

        private const int STACK_ALLOC_LIMIT = 1024;
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
            return new(pixel[2], pixel[1], pixel[0]);
        }

        /// <inheritdoc />
        public override Color this[in Rectangle rectangle]
        {
            get
            {
                if (Data.Length == 0) return Color.Transparent;

                SKRectI skRectI = CreatedFlooredRectI(
                    Size.Width * rectangle.Location.X.Clamp(0, 1),
                    Size.Height * rectangle.Location.Y.Clamp(0, 1),
                    Size.Width * rectangle.Size.Width.Clamp(0, 1),
                    Size.Height * rectangle.Size.Height.Clamp(0, 1)
                );

                if (skRectI.Width == 0 || skRectI.Height == 0) return Color.Transparent;
                if (skRectI.Width == 1 && skRectI.Height == 1) return GetColor(GetPixelData(skRectI.Left, skRectI.Top));

                int bufferSize = skRectI.Width * skRectI.Height * DATA_PER_PIXEL;
                if (bufferSize <= STACK_ALLOC_LIMIT)
                {
                    Span<byte> buffer = stackalloc byte[bufferSize];
                    GetRegionData(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height, buffer);

                    Span<byte> pixelData = stackalloc byte[DATA_PER_PIXEL];
                    Sampler.SampleColor(new SamplerInfo<byte>(skRectI.Width, skRectI.Height, buffer), pixelData);

                    return GetColor(pixelData);
                }
                else
                {
                    byte[] rent = ArrayPool<byte>.Shared.Rent(bufferSize);

                    Span<byte> buffer = new Span<byte>(rent).Slice(0, bufferSize);
                    GetRegionData(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height, buffer);

                    Span<byte> pixelData = stackalloc byte[DATA_PER_PIXEL];
                    Sampler.SampleColor(new SamplerInfo<byte>(skRectI.Width, skRectI.Height, buffer), pixelData);

                    ArrayPool<byte>.Shared.Return(rent);

                    return GetColor(pixelData);
                }
            }
        }

        private static SKRectI CreatedFlooredRectI(float x, float y, float width, float height)
        {
            return new(
                width <= 0.0 ? checked((int) Math.Floor(x)) : checked((int) Math.Ceiling(x)),
                height <= 0.0 ? checked((int) Math.Floor(y)) : checked((int) Math.Ceiling(y)),
                width >= 0.0 ? checked((int) Math.Floor(x + width)) : checked((int) Math.Ceiling(x + width)),
                height >= 0.0 ? checked((int) Math.Floor(y + height)) : checked((int) Math.Ceiling(y + height))
            );
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
}