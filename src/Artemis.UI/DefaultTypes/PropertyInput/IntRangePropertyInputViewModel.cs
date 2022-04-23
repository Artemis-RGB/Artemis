using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class IntRangePropertyInputViewModel : PropertyInputViewModel<IntRange>
{
    public IntRangePropertyInputViewModel(LayerProperty<IntRange> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        this.ValidationRule(vm => vm.Start, start => start <= End, "Start value must be less than the end value.");
        this.ValidationRule(vm => vm.End, end => end >= Start, "End value must be greater than the start value.");

        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
        {
            this.ValidationRule(vm => vm.Start, i => i >= Convert.ToInt32(LayerProperty.PropertyDescription.MinInputValue),
                $"Start value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            this.ValidationRule(vm => vm.End, i => i >= Convert.ToInt32(LayerProperty.PropertyDescription.MinInputValue),
                $"End value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            this.ValidationRule(vm => vm.Start, i => i < Convert.ToInt32(LayerProperty.PropertyDescription.MaxInputValue),
                $"Start value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            this.ValidationRule(vm => vm.End, i => i < Convert.ToInt32(LayerProperty.PropertyDescription.MaxInputValue),
                $"End value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    public int Start
    {
        get => InputValue?.Start ?? 0;
        set
        {
            if (InputValue == null)
                InputValue = new IntRange(value, value + 1);
            else
                InputValue.Start = value;

            this.RaisePropertyChanged(nameof(Start));
        }
    }

    public int End
    {
        get => InputValue?.End ?? 0;
        set
        {
            if (InputValue == null)
                InputValue = new IntRange(value - 1, value);
            else
                InputValue.End = value;

            this.RaisePropertyChanged(nameof(End));
        }
    }


    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(Start));
        this.RaisePropertyChanged(nameof(End));
    }
}