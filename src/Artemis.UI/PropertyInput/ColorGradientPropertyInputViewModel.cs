using System;
using Artemis.Core.Models.Profile.Colors;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.PropertyInput
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