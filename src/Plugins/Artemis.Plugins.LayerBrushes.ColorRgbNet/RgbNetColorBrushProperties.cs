using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrushProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The color of the brush")]
        public SKColorLayerProperty Color { get; set; }

        [PropertyDescription(InputPrefix = "Test")]
        public LayerProperty<string> TestProperty { get; set; }

        protected override void PopulateDefaults()
        {
            Color.DefaultValue = new SKColor(255, 0, 0);
            TestProperty.DefaultValue = "I was empty before!";
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}