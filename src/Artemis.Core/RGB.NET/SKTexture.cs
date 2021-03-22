using System;
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
        private readonly SKPixmap _pixelData;
        private readonly IntPtr _pixelDataPtr;

        #region Constructors

        internal SKTexture(IManagedGraphicsContext? graphicsContext, int width, int height, float scale) : base(width, height, 4, new AverageByteSampler())
        {
            ImageInfo = new SKImageInfo(width, height);
            Surface = graphicsContext == null 
                ? SKSurface.Create(ImageInfo) 
                : SKSurface.Create(graphicsContext.GraphicsContext, true, ImageInfo);
            RenderScale = scale;

            _pixelDataPtr = Marshal.AllocHGlobal(ImageInfo.BytesSize);
            _pixelData = new SKPixmap(ImageInfo, _pixelDataPtr, ImageInfo.RowBytes);
        }

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

        #region IDisposable

        private void ReleaseUnmanagedResources()
        {
            Marshal.FreeHGlobal(_pixelDataPtr);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Surface.Dispose();
            _pixelData.Dispose();
            
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~SKTexture()
        {
            ReleaseUnmanagedResources();
        }

        #endregion
    }
}