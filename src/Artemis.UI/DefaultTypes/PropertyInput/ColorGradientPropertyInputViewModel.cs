using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public class ColorGradientPropertyInputViewModel : PropertyInputViewModel<ColorGradient>
    {
        public ColorGradientPropertyInputViewModel(LayerProperty<ColorGradient> layerProperty, IProfileEditorService profileEditorService)
            : base(layerProperty, profileEditorService)
        {
        }

        public void DialogClosed(object sender, EventArgs e)
        {
            ApplyInputValue();
        }
    }
}