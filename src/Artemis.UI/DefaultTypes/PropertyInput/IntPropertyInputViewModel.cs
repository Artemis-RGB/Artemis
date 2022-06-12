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
            int minInputValue = Convert.ToInt32(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i >= minInputValue, $"Value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            MinInputValue = minInputValue;
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            int maxInputValue = Convert.ToInt32(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i < maxInputValue, $"Value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            MaxInputValue = maxInputValue;
        }
    }

    public int MinInputValue { get; } = int.MinValue;
    public int MaxInputValue { get; } = int.MaxValue;
}