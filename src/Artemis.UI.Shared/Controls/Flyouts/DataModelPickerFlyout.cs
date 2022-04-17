using System;
using System.ComponentModel;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;

namespace Artemis.UI.Shared.Flyouts;

/// <summary>
///     Defines a flyout that hosts a data model picker.
/// </summary>
public sealed class DataModelPickerFlyout : PickerFlyoutBase
{
    private DataModelPicker.DataModelPicker? _picker;

    /// <summary>
    ///     Gets the data model picker that the flyout hosts.
    /// </summary>
    public DataModelPicker.DataModelPicker DataModelPicker => _picker ??= new DataModelPicker.DataModelPicker();

    /// <summary>
    /// Raised when the Confirmed button is tapped indicating the new Color should be applied
    /// </summary>
    public event TypedEventHandler<DataModelPickerFlyout, object>? Confirmed;

    /// <summary>
    /// Raised when the Dismiss button is tapped, indicating the new color should not be applied
    /// </summary>
    public event TypedEventHandler<DataModelPickerFlyout, object>? Dismissed;

    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        _picker ??= new DataModelPicker.DataModelPicker();
        PickerFlyoutPresenter presenter = new() {Content = DataModelPicker};
        presenter.Confirmed += OnFlyoutConfirmed;
        presenter.Dismissed += OnFlyoutDismissed;

        return presenter;
    }

    /// <inheritdoc />
    protected override void OnConfirmed()
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    /// <inheritdoc />
    protected override void OnOpening(CancelEventArgs args)
    {
        base.OnOpening(args);
        (Popup.Child as PickerFlyoutPresenter)?.Classes.Set(":acceptdismiss", true);
    }

    private void OnFlyoutDismissed(PickerFlyoutPresenter sender, object args)
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void OnFlyoutConfirmed(PickerFlyoutPresenter sender, object args)
    {
        OnConfirmed();
    }
}