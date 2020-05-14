using System;
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

        protected override void OnPropertiesInitialized()
        {
            // Populate defaults
            if (!GradientType.IsLoadedFromStorage)
                GradientType.BaseValue = LayerBrushes.Color.GradientType.Solid;
            if (!Color.IsLoadedFromStorage)
                Color.BaseValue = new SKColor(255, 0, 0);
            if (!Gradient.IsLoadedFromStorage)
            {
                Gradient.BaseValue = new ColorGradient();
                Gradient.BaseValue.MakeFabulous();
            }

            GradientType.BaseValueChanged += GradientTypeOnBaseValueChanged;
        }

        private void GradientTypeOnBaseValueChanged(object sender, EventArgs e)
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