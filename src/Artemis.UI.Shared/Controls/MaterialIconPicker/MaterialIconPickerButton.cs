using System;
using Artemis.UI.Shared.Flyouts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using Material.Icons;

namespace Artemis.UI.Shared.MaterialIconPicker;

/// <summary>
///     Represents a button that can be used to pick a data model path in a flyout.
/// </summary>
public class MaterialIconPickerButton : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the placeholder to show when nothing is selected.
    /// </summary>
    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<MaterialIconPicker, string>(nameof(Placeholder), "Click to select");

    /// <summary>
    ///     Gets or sets the current Material icon.
    /// </summary>
    public static readonly StyledProperty<MaterialIconKind?> ValueProperty =
        AvaloniaProperty.Register<MaterialIconPicker, MaterialIconKind?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets the desired flyout placement.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<FlyoutBase, PlacementMode>(nameof(Placement), PlacementMode.BottomEdgeAlignedLeft);

    private Button? _button;
    private MaterialIconPickerFlyout? _flyout;
    private bool _flyoutActive;

    /// <summary>
    ///     Gets or sets the placeholder to show when nothing is selected.
    /// </summary>
    public string Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    ///     Gets or sets the current Material icon.
    /// </summary>
    public MaterialIconKind? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets the desired flyout placement.
    /// </summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    ///     Raised when the flyout opens.
    /// </summary>
    public event TypedEventHandler<MaterialIconPickerButton, EventArgs>? FlyoutOpened;

    /// <summary>
    ///     Raised when the flyout closes.
    /// </summary>
    public event TypedEventHandler<MaterialIconPickerButton, EventArgs>? FlyoutClosed;

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_flyout == null)
            return;

        MaterialIconPickerFlyout flyout = new();
        flyout.FlyoutPresenterClasses.Add("material-icon-picker-presenter");
        flyout.MaterialIconPicker.Value = Value;
        flyout.Placement = Placement;
        
        flyout.Closed += FlyoutOnClosed;
        flyout.ShowAt(this);
        FlyoutOpened?.Invoke(this, EventArgs.Empty);

        void FlyoutOnClosed(object? closedSender, EventArgs closedEventArgs)
        {
            Value = flyout.MaterialIconPicker.Value;
            flyout.Closed -= FlyoutOnClosed;
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_button != null)
            _button.Click -= OnButtonClick;
        base.OnApplyTemplate(e);
        _button = e.NameScope.Find<Button>("MainButton");
        if (_button != null)
            _button.Click += OnButtonClick;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _flyout ??= new MaterialIconPickerFlyout();
        _flyout.Closed += OnFlyoutClosed;
    }

    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        if (_flyoutActive)
        {
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
            _flyoutActive = false;
        }
    }


    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_flyout != null)
            _flyout.Closed -= OnFlyoutClosed;
    }

    #endregion
}