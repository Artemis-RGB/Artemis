using SkiaSharp;

#pragma warning disable 8618

namespace Artemis.Core
{
    /// <summary>
    ///     Represents the transform properties of a layer
    /// </summary>
    public class LayerTransformProperties : LayerPropertyGroup
    {
        /// <summary>
        ///     The point at which the shape is attached to its position
        /// </summary>
        [PropertyDescription(Description = "The point at which the shape is attached to its position", InputStepSize = 0.001f)]
        public SKPointLayerProperty AnchorPoint { get; set; }

        /// <summary>
        ///     The position of the shape
        /// </summary>
        [PropertyDescription(Description = "The position of the shape", InputStepSize = 0.001f)]
        public SKPointLayerProperty Position { get; set; }

        /// <summary>
        ///     The scale of the shape
        /// </summary>
        [PropertyDescription(Description = "The scale of the shape", InputAffix = "%", MinInputValue = 0f)]
        public SKSizeLayerProperty Scale { get; set; }

        /// <summary>
        ///     The rotation of the shape in degree
        /// </summary>
        [PropertyDescription(Description = "The rotation of the shape in degrees", InputAffix = "°")]
        public FloatLayerProperty Rotation { get; set; }

        /// <summary>
        ///     The opacity of the shape
        /// </summary>
        [PropertyDescription(Description = "The opacity of the shape", InputAffix = "%", MinInputValue = 0f, MaxInputValue = 100f)]
        public FloatLayerProperty Opacity { get; set; }

        /// <inheritdoc />
        protected override void PopulateDefaults()
        {
            Scale.DefaultValue = new SKSize(100, 100);
            Opacity.DefaultValue = 100;
        }

        /// <inheritdoc />
        protected override void EnableProperties()
        {
        }

        /// <inheritdoc />
        protected override void DisableProperties()
        {
        }
    }
}