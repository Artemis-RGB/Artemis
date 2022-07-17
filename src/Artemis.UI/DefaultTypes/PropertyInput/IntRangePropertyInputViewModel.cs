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
            Min = Convert.ToInt32(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.Start, i => i >= Min, $"Start value must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            Max = Convert.ToInt32(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.End, i => i < Max, $"End value must be smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    public int Start
    {
        get => InputValue.Start;
        set
        {
            InputValue = new IntRange(value, InputValue.End);
            this.RaisePropertyChanged(nameof(Start));
        }
    }

    public int End
    {
        get => InputValue.End;
        set
        {
            InputValue = new IntRange(InputValue.Start, value);
            this.RaisePropertyChanged(nameof(End));
        }
    }

    public int Min { get; } = int.MinValue;
    public int Max { get; } = int.MaxValue;

    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(Start));
        this.RaisePropertyChanged(nameof(End));
    }
}