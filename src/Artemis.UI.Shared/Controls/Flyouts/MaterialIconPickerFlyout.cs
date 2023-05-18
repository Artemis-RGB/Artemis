using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Shared.Flyouts;

/// <summary>
///     Defines a flyout that hosts a data model picker.
/// </summary>
public sealed class MaterialIconPickerFlyout : Flyout
{
    private MaterialIconPicker.MaterialIconPicker? _picker;

    /// <summary>
    ///     Gets the data model picker that the flyout hosts.
    /// </summary>
    public MaterialIconPicker.MaterialIconPicker MaterialIconPicker => _picker ??= new MaterialIconPicker.MaterialIconPicker();
    
    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        _picker ??= new MaterialIconPicker.MaterialIconPicker();
        PickerFlyoutPresenter presenter = new() {Content = MaterialIconPicker};
        return presenter;
    }
}