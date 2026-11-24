using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class FolderBlendModePropertyInputViewModel : PropertyInputViewModel<FolderBlendMode>
{
    public FolderBlendModePropertyInputViewModel(LayerProperty<FolderBlendMode> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }
}
