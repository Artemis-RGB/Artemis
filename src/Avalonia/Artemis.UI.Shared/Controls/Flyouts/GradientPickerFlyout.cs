using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Artemis.UI.Shared.Controls.Flyouts;

/// <summary>
///     Defines a flyout that hosts a gradient picker.
/// </summary>
public sealed class GradientPickerFlyout : Flyout
{
    private GradientPicker.GradientPicker? _picker;

    /// <summary>
    ///     Gets the gradient picker that this flyout hosts
    /// </summary>
    public GradientPicker.GradientPicker GradientPicker => _picker ??= new GradientPicker.GradientPicker();

    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        _picker ??= new GradientPicker.GradientPicker();
        FlyoutPresenter presenter = new() {Content = GradientPicker};
        return presenter;
    }
}