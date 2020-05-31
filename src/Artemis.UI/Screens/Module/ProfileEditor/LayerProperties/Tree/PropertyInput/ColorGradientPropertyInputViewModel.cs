using System;
using Artemis.Core.Models.Profile.Colors;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class ColorGradientPropertyInputViewModel : PropertyInputViewModel<ColorGradient>
    {
        public ColorGradientPropertyInputViewModel(LayerPropertyViewModel<ColorGradient> layerPropertyViewModel) : base(layerPropertyViewModel)
        {
        }

        public void DialogClosed(object sender, EventArgs e)
        {
            ApplyInputValue();
        }
    }
}