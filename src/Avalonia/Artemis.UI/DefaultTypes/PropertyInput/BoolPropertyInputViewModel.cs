using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class BoolPropertyInputViewModel : PropertyInputViewModel<bool>
{
    public BoolPropertyInputViewModel(LayerProperty<bool> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }
}