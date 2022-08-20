using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class EnumPropertyInputViewModel<T> : PropertyInputViewModel<T> where T : Enum
{
    public EnumPropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        // TODO: Test if WhenActivated works here
    }

    public Type EnumType => typeof(T);
}