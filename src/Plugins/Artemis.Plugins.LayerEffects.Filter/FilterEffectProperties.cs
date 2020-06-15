using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription]
        public EnumLayerProperty<FilterType> FilterType { get; set; }

        [PropertyDescription(Description = "The amount of blur to apply")]
        public SKSizeLayerProperty BlurAmount { get; set; }

        [PropertyDescription(Description = "The amount of dilation to apply")]
        public SKSizeLayerProperty DilateRadius { get; set; }

        [PropertyDescription(Description = "The amount of erode to apply")]
        public SKSizeLayerProperty ErodeRadius { get; set; }

        [PropertyDescription(Description = "The offset of the shadow")]
        public SKPointLayerProperty ShadowOffset { get; set; }

        [PropertyDescription(Description = "The amount of blur to apply to the shadow")]
        public SKSizeLayerProperty ShadowBlurAmount { get; set; }

        [PropertyDescription(Description = "The color of the shadow")]
        public SKColorLayerProperty ShadowColor { get; set; }

        protected override void PopulateDefaults()
        {
            ShadowBlurAmount.DefaultValue = new SKSize(5, 5);
            ShadowColor.DefaultValue = new SKColor(50, 50, 50);
        }

        protected override void OnPropertiesInitialized()
        {
            FilterType.BaseValueChanged += (sender, args) => UpdateVisibility();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            BlurAmount.IsHidden = FilterType != Filter.FilterType.Blur;
            DilateRadius.IsHidden = FilterType != Filter.FilterType.Dilate;
            ErodeRadius.IsHidden = FilterType != Filter.FilterType.Erode;
            ShadowOffset.IsHidden = FilterType != Filter.FilterType.DropShadow;
            ShadowBlurAmount.IsHidden = FilterType != Filter.FilterType.DropShadow;
            ShadowColor.IsHidden = FilterType != Filter.FilterType.DropShadow;
        }
    }

    public enum FilterType
    {
        Blur,
        Dilate,
        Erode,
        DropShadow,
    }
}