using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class IntPropertyInputViewModel : PropertyInputViewModel<int>
{
    public IntPropertyInputViewModel(LayerProperty<int> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
        {
            Min = Convert.ToInt32(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i >= Min, $"Value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            Max = Convert.ToInt32(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i < Max, $"Value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    public int Min { get; } = int.MinValue;
    public int Max { get; } = int.MaxValue;
}