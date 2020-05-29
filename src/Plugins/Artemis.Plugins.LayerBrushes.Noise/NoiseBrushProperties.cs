using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrushProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Color mapping type", Description = "The way the noise is converted to colors")]
        public EnumLayerProperty<ColorMappingType> ColorType { get; set; }

        [PropertyDescription(Description = "The main color of the noise")]
        public SKColorLayerProperty MainColor { get; set; }

        [PropertyDescription(Description = "The secondary color of the noise")]
        public SKColorLayerProperty SecondaryColor { get; set; }

        [PropertyDescription(Name = "Noise gradient map", Description = "The gradient the noise will map it's value to")]
        public ColorGradientLayerProperty GradientColor { get; set; }


        [PropertyDescription(Description = "The scale of the noise", MinInputValue = 0f, InputAffix = "%")]
        public SKSizeLayerProperty Scale { get; set; }

        [PropertyDescription(Description = "The hardness of the noise, lower means there are gradients in the noise, higher means hard lines", MinInputValue = 0f, MaxInputValue = 2048f)]
        public FloatLayerProperty Hardness { get; set; }

        [PropertyDescription(Description = "The speed at which the noise moves vertically and horizontally", MinInputValue = -64f, MaxInputValue = 64f)]
        public SKPointLayerProperty ScrollSpeed { get; set; }

        [PropertyDescription(Description = "The speed at which the noise moves", MinInputValue = 0f, MaxInputValue = 64f)]
        public FloatLayerProperty AnimationSpeed { get; set; }

        protected override void PopulateDefaults()
        {
            MainColor.DefaultValue = new SKColor(255, 0, 0);
            SecondaryColor.DefaultValue = new SKColor(0, 0, 255);
            GradientColor.DefaultValue = ColorGradient.GetUnicornBarf();
            Scale.DefaultValue = new SKSize(100, 100);
            Hardness.DefaultValue = 500f;
            AnimationSpeed.DefaultValue = 25f;
        }

        protected override void OnPropertiesInitialized()
        {
            ColorType.BaseValueChanged += (sender, args) => UpdateVisibility();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            GradientColor.IsHidden = ColorType.BaseValue != ColorMappingType.Gradient;
            MainColor.IsHidden = ColorType.BaseValue != ColorMappingType.Simple;
            SecondaryColor.IsHidden = ColorType.BaseValue != ColorMappingType.Simple;
        }
    }
}