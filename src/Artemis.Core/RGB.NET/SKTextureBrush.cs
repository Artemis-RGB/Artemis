using RGB.NET.Core;

namespace Artemis.Core
{
    internal class SKTextureBrush : AbstractBrush
    {
        #region Properties & Fields

        private SKTexture? _texture;
        /// <summary>
        /// Gets or sets the texture drawn by this brush.
        /// </summary>
        public SKTexture? Texture
        {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureBrush" /> class.
        /// </summary>
        /// <param name="texture">The texture drawn by this brush.</param>
        public SKTextureBrush(SKTexture? texture)
        {
            this.Texture = texture;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Color GetColorAtPoint(in Rectangle rectangle, in RenderTarget renderTarget)
        {
            return Texture?.GetColorAtRenderTarget(renderTarget) ?? Color.Transparent;
        }

        #endregion
    }
}
