using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class FloatRangePropertyInputViewModel : PropertyInputViewModel<FloatRange>
{
    public FloatRangePropertyInputViewModel(LayerProperty<FloatRange> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        this.ValidationRule(vm => vm.Start, start => start <= End, "Start value must be less than the end value.");
        this.ValidationRule(vm => vm.End, end => end >= Start, "End value must be greater than the start value.");

        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
        {
            Min = Convert.ToSingle(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.Start, i => i >= Min, $"Start value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            Max = Convert.ToSingle(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.End, i => i < Max, $"End value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    public float Start
    {
        get => InputValue.Start;
        set
        {
            InputValue = new FloatRange(value, InputValue.End);
            this.RaisePropertyChanged(nameof(Start));
        }
    }

    public float End
    {
        get => InputValue.End;
        set
        {
            InputValue = new FloatRange(InputValue.Start, value);
            this.RaisePropertyChanged(nameof(End));
        }
    }

    public float Min { get; } = float.MinValue;
    public float Max { get; } = float.MaxValue;

    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(Start));
        this.RaisePropertyChanged(nameof(End));
    }
}