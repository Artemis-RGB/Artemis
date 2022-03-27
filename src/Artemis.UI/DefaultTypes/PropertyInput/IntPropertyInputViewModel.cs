﻿using Artemis.Core;
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
            this.ValidationRule(vm => vm.InputValue, i => i >= (int) LayerProperty.PropertyDescription.MinInputValue,
                $"Value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
            this.ValidationRule(vm => vm.InputValue, i => i < (int) LayerProperty.PropertyDescription.MaxInputValue, 
                $"Value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
    }
}