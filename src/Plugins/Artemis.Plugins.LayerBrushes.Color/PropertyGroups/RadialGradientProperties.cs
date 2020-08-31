using System.ComponentModel;
using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Color.PropertyGroups
{
    public class RadialGradientProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Center offset", Description = "Change the position of the gradient by offsetting it from the center of the layer", InputAffix = "%")]
        public SKPointLayerProperty CenterOffset { get; set; }

        [PropertyDescription(Name = "Resize mode", Description = "How to make the gradient adjust to scale changes")]
        public EnumLayerProperty<RadialGradientResizeMode> ResizeMode { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void EnableProperties()
        {
            if (ProfileElement is Layer layer)
                layer.General.ResizeMode.BaseValueChanged += OnBaseValueChanged;

            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
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

            ResizeMode.IsHidden = !normalRender;
        }

        public enum RadialGradientResizeMode
        {
            [Description("Stretch or shrink")]
            Stretch,

            [Description("Maintain a circle")]
            MaintainCircle
        }
    }
}