using Artemis.UI.Shared.Controls.GradientPicker;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Flyouts;

/// <summary>
///     Defines a flyout that hosts a gradient picker.
/// </summary>
public sealed class GradientPickerFlyout : Flyout
{
    private GradientPicker? _picker;

    /// <summary>
    ///     Gets the gradient picker that this flyout hosts.
    /// </summary>
    public GradientPicker GradientPicker => _picker ??= new GradientPicker();

    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        _picker ??= new GradientPicker();
        FlyoutPresenter presenter = new() {Content = GradientPicker};
        return presenter;
    }
}