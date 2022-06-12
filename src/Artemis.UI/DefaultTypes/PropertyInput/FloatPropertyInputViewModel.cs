using System;
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
        {
            float minInputValue = Convert.ToSingle(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i >= minInputValue, $"Value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            MinInputValue = minInputValue;
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            float maxInputValue = Convert.ToSingle(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.InputValue, i => i <= maxInputValue, $"Value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            MaxInputValue = maxInputValue;
        }
    }

    public float MinInputValue { get; } = float.MinValue;
    public float MaxInputValue { get; } = float.MaxValue;
}