using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public class LayerTransformProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The point at which the shape is attached to its position", InputStepSize = 0.001f)]
        public SKPointLayerProperty AnchorPoint { get; set; }

        [PropertyDescription(Description = "The position of the shape", InputStepSize = 0.001f)]
        public SKPointLayerProperty Position { get; set; }

        [PropertyDescription(Description = "The scale of the shape", InputAffix = "%", MinInputValue = 0f)]
        public SKSizeLayerProperty Scale { get; set; }

        [PropertyDescription(Description = "The rotation of the shape in degrees", InputAffix = "°")]
        public FloatLayerProperty Rotation { get; set; }

        [PropertyDescription(Description = "The opacity of the shape", InputAffix = "%", MinInputValue = 0f, MaxInputValue = 100f)]
        public FloatLayerProperty Opacity { get; set; }

        protected override void PopulateDefaults()
        {
            Scale.DefaultValue = new SKSize(100, 100);
            Opacity.DefaultValue = 100;
        }

        protected override void EnableProperties()
        {
        }
    }
}