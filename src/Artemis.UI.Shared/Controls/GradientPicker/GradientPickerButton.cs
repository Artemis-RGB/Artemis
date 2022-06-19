using System;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Flyouts;
using Artemis.UI.Shared.Providers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.Core;
using Button = FluentAvalonia.UI.Controls.Button;

namespace Artemis.UI.Shared.Controls.GradientPicker;

/// <summary>
///     Represents a gradient picker box that can be used to edit a gradient
/// </summary>
public class GradientPickerButton : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the color gradient.
    /// </summary>
    public static readonly StyledProperty<ColorGradient?> ColorGradientProperty =
        AvaloniaProperty.Register<GradientPickerButton, ColorGradient?>(nameof(ColorGradient), notifying: ColorGradientChanged, defaultValue: ColorGradient.GetUnicornBarf());

    /// <summary>
    ///     Gets or sets a boolean indicating whether the gradient picker should be in compact mode or not.
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<GradientPickerButton, bool>(nameof(IsCompact), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets a storage provider to use for storing and loading gradients.
    /// </summary>
    public static readonly StyledProperty<IColorGradientStorageProvider?> StorageProviderProperty =
        AvaloniaProperty.Register<GradientPickerButton, IColorGradientStorageProvider?>(nameof(StorageProvider));

    /// <summary>
    ///     Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public static readonly DirectProperty<GradientPickerButton, LinearGradientBrush> LinearGradientBrushProperty =
        AvaloniaProperty.RegisterDirect<GradientPickerButton, LinearGradientBrush>(nameof(LinearGradientBrush), g => g.LinearGradientBrush);

    private ColorGradient? _lastColorGradient;
    private Button? _button;
    private GradientPickerFlyout? _flyout;
    private bool _flyoutActive;

    /// <summary>
    ///     Gets or sets the color gradient.
    /// </summary>
    public ColorGradient? ColorGradient
    {
        get => GetValue(ColorGradientProperty);
        set => SetValue(ColorGradientProperty, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the gradient picker should be in compact mode or not.
    /// </summary>
    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    ///     Gets or sets a storage provider to use for storing and loading gradients.
    /// </summary>
    public IColorGradientStorageProvider? StorageProvider
    {
        get => GetValue(StorageProviderProperty);
        set => SetValue(StorageProviderProperty, value);
    }

    /// <summary>
    ///     Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public LinearGradientBrush LinearGradientBrush { get; } = new();

    /// <summary>
    ///     Raised when the flyout opens.
    /// </summary>
    public event TypedEventHandler<GradientPickerButton, EventArgs>? FlyoutOpened;

    /// <summary>
    ///     Raised when the flyout closes.
    /// </summary>
    public event TypedEventHandler<GradientPickerButton, EventArgs>? FlyoutClosed;

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

    #endregion

    private static void ColorGradientChanged(IAvaloniaObject sender, bool before)
    {
        (sender as GradientPickerButton)?.Subscribe();
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (ColorGradient != null)
        {
            ColorGradient.CollectionChanged += ColorGradientOnCollectionChanged;
            ColorGradient.StopChanged += ColorGradientOnStopChanged;
        }

        UpdateGradient();
        _lastColorGradient = ColorGradient;
    }

    private void Unsubscribe()
    {
        if (_lastColorGradient == null)
            return;

        _lastColorGradient.CollectionChanged -= ColorGradientOnCollectionChanged;
        _lastColorGradient.StopChanged -= ColorGradientOnStopChanged;
        _lastColorGradient = null;
    }

    private void ColorGradientOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateGradient();
    }

    private void ColorGradientOnStopChanged(object? sender, EventArgs e)
    {
        UpdateGradient();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_flyout == null || ColorGradient == null)
            return;

        // Logic here is taken from Fluent Avalonia's ColorPicker which also reuses the same control since it's large
        _flyout.GradientPicker.ColorGradient = ColorGradient;
        _flyout.GradientPicker.IsCompact = IsCompact;
        _flyout.GradientPicker.StorageProvider = StorageProvider;

        _flyout.ShowAt(this);
        _flyoutActive = true;

        FlyoutOpened?.Invoke(this, EventArgs.Empty);
    }


    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        if (_flyoutActive)
        {
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
            _flyoutActive = false;
        }
    }

    private void UpdateGradient()
    {
        // Update the display gradient
        GradientStops collection = new();
        if (ColorGradient != null)
        {
            foreach (ColorGradientStop c in ColorGradient.OrderBy(s => s.Position))
                collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));
        }

        LinearGradientBrush.GradientStops = collection;
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Subscribe();

        if (_flyout == null)
        {
            _flyout = new GradientPickerFlyout();
            _flyout.FlyoutPresenterClasses.Add("gradient-picker-presenter");
        }

        _flyout.Closed += OnFlyoutClosed;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        if (_flyout != null)
            _flyout.Closed -= OnFlyoutClosed;
    }

    #endregion
}