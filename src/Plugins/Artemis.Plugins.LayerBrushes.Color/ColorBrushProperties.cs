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

        [PropertyDescription(Description = "How to handle the layer having to stretch beyond it's regular size")]
        public EnumLayerProperty<SKShaderTileMode> GradientTileMode { get; set; }

        [PropertyDescription(Description = "The color of the brush")]
        public SKColorLayerProperty Color { get; set; }

        [PropertyDescription(Description = "The gradient of the brush")]
        public ColorGradientLayerProperty Gradient { get; set; }

        [PropertyDescription(KeyframesSupported = false, Description = "How many times to repeat the colors in the selected gradient", MinInputValue = 0, MaxInputValue = 10)]
        public IntLayerProperty GradientRepeat { get; set; }

        protected override void PopulateDefaults()
        {
            GradientType.DefaultValue = LayerBrushes.Color.GradientType.Solid;
            Color.DefaultValue = new SKColor(255, 0, 0);
            Gradient.DefaultValue = ColorGradient.GetUnicornBarf();
            GradientRepeat.DefaultValue = 0;
        }

        protected override void OnPropertiesInitialized()
        {
            GradientType.BaseValueChanged += (sender, args) => UpdateVisibility();
            if (ProfileElement is Layer layer)
                layer.General.FillType.BaseValueChanged += (sender, args) => UpdateVisibility();

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            Color.IsHidden = GradientType.BaseValue != LayerBrushes.Color.GradientType.Solid;
            Gradient.IsHidden = GradientType.BaseValue == LayerBrushes.Color.GradientType.Solid;
            GradientRepeat.IsHidden = GradientType.BaseValue == LayerBrushes.Color.GradientType.Solid;

            if (ProfileElement is Layer layer)
                GradientTileMode.IsHidden = layer.General.FillType.CurrentValue != LayerFillType.Clip;
            else
                GradientTileMode.IsHidden = true;
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