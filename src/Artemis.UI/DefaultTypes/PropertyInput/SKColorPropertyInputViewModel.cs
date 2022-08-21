using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using SkiaSharp;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class SKColorPropertyInputViewModel : PropertyInputViewModel<SKColor>
{
    public SKColorPropertyInputViewModel(LayerProperty<SKColor> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }
}