using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using SkiaSharp;

// using PropertyChanged;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class SKSizePropertyInputViewModel : PropertyInputViewModel<SKSize>
{
    public SKSizePropertyInputViewModel(LayerProperty<SKSize> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        if (LayerProperty.PropertyDescription.MinInputValue.IsNumber())
        {
            this.ValidationRule(vm => vm.Width, i => i >= (float) LayerProperty.PropertyDescription.MinInputValue,
                $"Width must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
            this.ValidationRule(vm => vm.Height, i => i >= (float) LayerProperty.PropertyDescription.MinInputValue,
                $"Height must be equal to or greater than {LayerProperty.PropertyDescription.MinInputValue}.");
        }

        if (LayerProperty.PropertyDescription.MaxInputValue.IsNumber())
        {
            this.ValidationRule(vm => vm.Width, i => i <= (float) LayerProperty.PropertyDescription.MaxInputValue,
                $"Width must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
            this.ValidationRule(vm => vm.Height, i => i <= (float) LayerProperty.PropertyDescription.MaxInputValue,
                $"Height must be equal to or smaller than {LayerProperty.PropertyDescription.MaxInputValue}.");
        }
    }

    // Since SKSize is immutable we need to create properties that replace the SKSize entirely
    public float Width
    {
        get => InputValue.Width;
        set => InputValue = new SKSize(value, Height);
    }

    public float Height
    {
        get => InputValue.Height;
        set => InputValue = new SKSize(Width, value);
    }

    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
    }
}