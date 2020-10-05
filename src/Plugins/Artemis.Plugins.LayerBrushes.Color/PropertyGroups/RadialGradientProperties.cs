using System.ComponentModel;
using Artemis.Core;
using Artemis.Core.DefaultTypes;

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
                layer.General.ResizeMode.CurrentValueSet += ResizeModeOnCurrentValueSet;

            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
            if (ProfileElement is Layer layer)
                layer.General.ResizeMode.CurrentValueSet -= ResizeModeOnCurrentValueSet;
        }

        private void ResizeModeOnCurrentValueSet(object sender, LayerPropertyEventArgs<LayerResizeMode> e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            bool normalRender = false;
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