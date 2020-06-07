using System.ComponentModel;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The type of color brush to draw")]
        public EnumLayerProperty<GradientType> GradientType { get; set; }

        [PropertyDescription(Description = "The color of the brush")]
        public SKColorLayerProperty Color { get; set; }

        [PropertyDescription(Description = "The gradient of the brush")]
        public ColorGradientLayerProperty Gradient { get; set; }

        protected override void PopulateDefaults()
        {
            GradientType.DefaultValue = LayerBrushes.Color.GradientType.Solid;
            Color.DefaultValue = new SKColor(255, 0, 0);
            Gradient.DefaultValue = ColorGradient.GetUnicornBarf();
        }

        protected override void OnPropertiesInitialized()
        {
            GradientType.BaseValueChanged += (sender, args) => UpdateVisibility();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            Color.IsHidden = GradientType.BaseValue != LayerBrushes.Color.GradientType.Solid;
            Gradient.IsHidden = GradientType.BaseValue == LayerBrushes.Color.GradientType.Solid;
        }
    }

    public enum GradientType
    {
        [Description("Solid")]
        Solid,

        [Description("Linear Gradient")]
        LinearGradient,

        [Description("Radial Gradient")]
        RadialGradient,

        [Description("Sweep Gradient")]
        SweepGradient
    }
}