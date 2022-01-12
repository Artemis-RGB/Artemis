using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class ColorGradientPropertyInputViewModel : PropertyInputViewModel<ColorGradient>
{
    public ColorGradientPropertyInputViewModel(LayerProperty<ColorGradient> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }

    public void DialogClosed(object sender, EventArgs e)
    {
        ApplyInputValue();
    }
}