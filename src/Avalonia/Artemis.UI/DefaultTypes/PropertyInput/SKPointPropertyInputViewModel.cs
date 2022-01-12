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
            this.ValidationRule(vm => vm.X, i => i >= (float) LayerProperty.PropertyDescription.MinInputValue,
                $"X must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            this.ValidationRule(vm => vm.Y, i => i >= (float) LayerProperty.PropertyDescription.MinInputValue,
                $"Y must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            this.ValidationRule(vm => vm.X, i => i <= (float) LayerProperty.PropertyDescription.MaxInputValue,
                $"X must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            this.ValidationRule(vm => vm.Y, i => i <= (float) LayerProperty.PropertyDescription.MaxInputValue,
                $"Y must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
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


    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
}