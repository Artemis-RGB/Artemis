﻿using Artemis.Core;
using Artemis.Core.DefaultTypes;
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

        [PropertyDescription(Description = "The hardness of the noise, lower means there are gradients in the noise, higher means hard lines", InputAffix = "%", MinInputValue = 0f,
            MaxInputValue = 400)]
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
            Hardness.DefaultValue = 100f;
            AnimationSpeed.DefaultValue = 25f;
        }

        protected override void EnableProperties()
        {
            ColorType.CurrentValueSet += ColorTypeOnCurrentValueSet;
            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
            ColorType.CurrentValueSet -= ColorTypeOnCurrentValueSet;
        }

        private void ColorTypeOnCurrentValueSet(object sender, LayerPropertyEventArgs<ColorMappingType> e)
        {
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