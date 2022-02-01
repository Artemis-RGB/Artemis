using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class StringPropertyInputViewModel : PropertyInputViewModel<string>
{
    public StringPropertyInputViewModel(LayerProperty<string> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }
}