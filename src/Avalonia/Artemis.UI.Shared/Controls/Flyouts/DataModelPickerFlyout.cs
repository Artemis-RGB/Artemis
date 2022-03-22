using Avalonia.Controls;

namespace Artemis.UI.Shared.Controls.Flyouts;

/// <summary>
///     Defines a flyout that hosts a data model picker.
/// </summary>
public sealed class DataModelPickerFlyout : Flyout
{
    private DataModelPicker.DataModelPicker? _picker;

    /// <summary>
    ///     Gets the data model picker that the flyout hosts.
    /// </summary>
    public DataModelPicker.DataModelPicker DataModelPicker => _picker ??= new DataModelPicker.DataModelPicker();

    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        _picker ??= new DataModelPicker.DataModelPicker();
        FlyoutPresenter presenter = new() {Content = DataModelPicker};
        return presenter;
    }
}