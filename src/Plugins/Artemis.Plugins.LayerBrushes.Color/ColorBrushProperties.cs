using System.ComponentModel;
using Artemis.Core.Events;
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

        [PropertyDescription(DisableKeyframes = true, Description = "How many times to repeat the colors in the selected gradient", MinInputValue = 0, MaxInputValue = 10)]
        public IntLayerProperty GradientRepeat { get; set; }

        #region Linear greadient properties

        [PropertyDescription(Name = "Rotation", Description = "Change the rotation of the linear gradient without affecting the rotation of the shape", InputAffix = "°")]
        public FloatLayerProperty LinearGradientRotation { get; set; }

        #endregion

        protected override void PopulateDefaults()
        {
            GradientType.DefaultValue = LayerBrushes.Color.GradientType.Solid;
            Color.DefaultValue = new SKColor(255, 0, 0);
            Gradient.DefaultValue = ColorGradient.GetUnicornBarf();
            GradientRepeat.DefaultValue = 0;
        }

        protected override void EnableProperties()
        {
            GradientType.BaseValueChanged += OnBaseValueChanged;
            if (ProfileElement is Layer layer)
                layer.General.ResizeMode.BaseValueChanged += OnBaseValueChanged;

            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
            GradientType.BaseValueChanged -= OnBaseValueChanged;
            if (ProfileElement is Layer layer)
                layer.General.ResizeMode.BaseValueChanged -= OnBaseValueChanged;
        }

        private void OnBaseValueChanged(object sender, LayerPropertyEventArgs e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            var normalRender = false;
            if (ProfileElement is Layer layer)
                normalRender = layer.General.ResizeMode.CurrentValue == LayerResizeMode.Normal;

            Color.IsHidden = GradientType.BaseValue != LayerBrushes.Color.GradientType.Solid;
            Gradient.IsHidden = GradientType.BaseValue == LayerBrushes.Color.GradientType.Solid;
            GradientRepeat.IsHidden = GradientType.BaseValue == LayerBrushes.Color.GradientType.Solid;

            RadialGradientCenterOffset.IsHidden = GradientType.BaseValue != LayerBrushes.Color.GradientType.RadialGradient;
            RadialGradientResizeMode.IsHidden = GradientType.BaseValue != LayerBrushes.Color.GradientType.RadialGradient;

            GradientTileMode.IsHidden = normalRender;
            RadialGradientResizeMode.IsHidden = !normalRender || GradientType.BaseValue != LayerBrushes.Color.GradientType.RadialGradient;
        }

        #region Radial gradient properties

        [PropertyDescription(Name = "Center offset", Description = "Change the position of the gradient by offsetting it from the center of the layer", InputAffix = "%")]
        public SKPointLayerProperty RadialGradientCenterOffset { get; set; }

        [PropertyDescription(Name = "Resize mode", Description = "How to make the gradient adjust to scale changes")]
        public EnumLayerProperty<RadialGradientResizeMode> RadialGradientResizeMode { get; set; }

        #endregion
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

    public enum RadialGradientResizeMode
    {
        [Description("Stretch or shrink")]
        Stretch,

        [Description("Maintain a circle")]
        MaintainCircle
    }
}