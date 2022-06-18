using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using SkiaSharp;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class SKPointPropertyInputViewModel : PropertyInputViewModel<SKPoint>
{
    public SKPointPropertyInputViewModel(LayerProperty<SKPoint> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
        {
            Min = Convert.ToSingle(LayerProperty.PropertyDescription.MinInputValue);
            this.ValidationRule(vm => vm.X, i => i >= Min, $"X must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            this.ValidationRule(vm => vm.Y, i => i >= Min, $"Y must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            Max = Convert.ToSingle(LayerProperty.PropertyDescription.MaxInputValue);
            this.ValidationRule(vm => vm.X, i => i <= Max, $"X must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            this.ValidationRule(vm => vm.Y, i => i <= Max, $"Y must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    public float X
    {
        get => InputValue.X;
        set => InputValue = new SKPoint(value, Y);
    }

    public float Y
    {
        get => InputValue.Y;
        set => InputValue = new SKPoint(X, value);
    }

    public float Min { get; } = float.MinValue;
    public float Max { get; } = float.MaxValue;

    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
}