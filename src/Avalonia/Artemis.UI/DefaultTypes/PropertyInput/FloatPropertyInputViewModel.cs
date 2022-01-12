using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class FloatPropertyInputViewModel : PropertyInputViewModel<float>
{
    public FloatPropertyInputViewModel(LayerProperty<float> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
            this.ValidationRule(vm => vm.InputValue, i => i >= (float) LayerProperty.PropertyDescription.MinInputValue,
                $"Value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
            this.ValidationRule(vm => vm.InputValue, i => i < (float) LayerProperty.PropertyDescription.MaxInputValue,
                $"Value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
    }
}