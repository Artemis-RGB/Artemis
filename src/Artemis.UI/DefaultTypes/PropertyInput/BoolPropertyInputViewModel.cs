using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class BoolPropertyInputViewModel : PropertyInputViewModel<bool>
{
    private BooleanOptions _selectedBooleanOption;

    public BoolPropertyInputViewModel(LayerProperty<bool> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        this.WhenAnyValue(vm => vm.InputValue).Subscribe(v => SelectedBooleanOption = v ? BooleanOptions.True : BooleanOptions.False);
        this.WhenAnyValue(vm => vm.SelectedBooleanOption).Subscribe(v => InputValue = v == BooleanOptions.True);
    }

    public BooleanOptions SelectedBooleanOption
    {
        get => _selectedBooleanOption;
        set => this.RaiseAndSetIfChanged(ref _selectedBooleanOption, value);
    }
}

public enum BooleanOptions
{
    True,
    False
}