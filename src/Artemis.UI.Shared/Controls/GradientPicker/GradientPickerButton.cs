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
using Avalonia.Threading;
using FluentAvalonia.Core;

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
        AvaloniaProperty.Register<GradientPickerButton, ColorGradient?>(nameof(ColorGradient));

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

    private Button? _button;
    private ColorGradient? _lastColorGradient;

    /// <inheritdoc />
    public GradientPickerButton()
    {
        PropertyChanged += OnPropertyChanged;
    }

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
    public LinearGradientBrush LinearGradientBrush { get; } = new() {StartPoint = RelativePoint.TopLeft, EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative)};

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
        Dispatcher.UIThread.Post(UpdateGradient);
    }

    private void ColorGradientOnStopChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(UpdateGradient);
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ColorGradient == null)
            return;

        GradientPickerFlyout flyout = new();
        flyout.FlyoutPresenterClasses.Add("gradient-picker-presenter");
        flyout.GradientPicker.ColorGradient = ColorGradient;
        flyout.GradientPicker.IsCompact = IsCompact;
        flyout.GradientPicker.StorageProvider = StorageProvider;

        flyout.Closed += FlyoutOnClosed;
        flyout.ShowAt(this);
        FlyoutOpened?.Invoke(this, EventArgs.Empty);

        void FlyoutOnClosed(object? closedSender, EventArgs closedEventArgs)
        {
            flyout.Closed -= FlyoutOnClosed;
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateGradient()
    {
        // Update the display gradient
        GradientStops collection = new();
        if (ColorGradient != null)
            foreach (ColorGradientStop c in ColorGradient.OrderBy(s => s.Position))
                collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));

        LinearGradientBrush.GradientStops = collection;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ColorGradientProperty)
            Subscribe();
    }
    
    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
    }

    #endregion
}