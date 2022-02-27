using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared.Providers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using ReactiveUI;
using Button = Avalonia.Controls.Button;

namespace Artemis.UI.Shared.Controls.GradientPicker;

/// <summary>
/// Represents a gradient picker that can be used to edit a gradient.
/// </summary>
public class GradientPicker : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the color gradient.
    /// </summary>
    public static readonly StyledProperty<ColorGradient> ColorGradientProperty =
        AvaloniaProperty.Register<GradientPicker, ColorGradient>(nameof(ColorGradient), notifying: ColorGradientChanged, defaultValue: ColorGradient.GetUnicornBarf());

    /// <summary>
    ///     Gets or sets the currently selected color stop.
    /// </summary>
    public static readonly StyledProperty<ColorGradientStop?> SelectedColorStopProperty =
        AvaloniaProperty.Register<GradientPicker, ColorGradientStop?>(nameof(SelectedColorStop), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets a boolean indicating whether the gradient picker should be in compact mode or not.
    /// </summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<GradientPicker, bool>(nameof(IsCompact), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets a storage provider to use for storing and loading gradients.
    /// </summary>
    public static readonly StyledProperty<IColorGradientStorageProvider?> StorageProviderProperty =
        AvaloniaProperty.Register<GradientPicker, IColorGradientStorageProvider?>(nameof(StorageProvider), notifying: StorageProviderChanged);

    /// <summary>
    ///     Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public static readonly DirectProperty<GradientPicker, LinearGradientBrush> LinearGradientBrushProperty =
        AvaloniaProperty.RegisterDirect<GradientPicker, LinearGradientBrush>(nameof(LinearGradientBrush), g => g.LinearGradientBrush);

    /// <summary>
    ///     Gets the command to execute when deleting stops.
    /// </summary>
    public static readonly DirectProperty<GradientPicker, ICommand> DeleteStopProperty =
        AvaloniaProperty.RegisterDirect<GradientPicker, ICommand>(nameof(DeleteStop), g => g.DeleteStop);

    private readonly ICommand _deleteStop;
    private Button? _flipStops;
    private Border? _gradient;
    private Button? _rotateStops;
    private bool _shiftDown;
    private Button? _spreadStops;
    private Button? _toggleSeamless;
    private ColorGradient? _lastColorGradient;
    private ColorPicker? _colorPicker;

    public GradientPicker()
    {
        _deleteStop = ReactiveCommand.Create<ColorGradientStop>(s =>
        {
            if (ColorGradient.Count <= 2)
                return;

            int index = ColorGradient.IndexOf(s);
            ColorGradient.Remove(s);
            if (index > ColorGradient.Count - 1)
                index--;

            SelectedColorStop = ColorGradient.ElementAtOrDefault(index);
        });
    }

    /// <summary>
    ///     Gets or sets the color gradient.
    /// </summary>
    public ColorGradient ColorGradient
    {
        get => GetValue(ColorGradientProperty);
        set => SetValue(ColorGradientProperty, value);
    }

    /// <summary>
    ///     Gets or sets the currently selected color stop.
    /// </summary>
    public ColorGradientStop? SelectedColorStop
    {
        get => GetValue(SelectedColorStopProperty);
        set
        {
            if (_colorPicker != null && SelectedColorStop != null)
                _colorPicker.PreviousColor = new Color2(SelectedColorStop.Color.Red, SelectedColorStop.Color.Green, SelectedColorStop.Color.Blue, SelectedColorStop.Color.Alpha);
            SetValue(SelectedColorStopProperty, value);
        }
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
    ///     Gets the command to execute when deleting stops.
    /// </summary>
    public ICommand DeleteStop
    {
        get => _deleteStop;
        private init => SetAndRaise(DeleteStopProperty, ref _deleteStop, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_gradient != null)
            _gradient.PointerPressed -= GradientOnPointerPressed;
        if (_spreadStops != null)
            _spreadStops.Click -= SpreadStopsOnClick;
        if (_toggleSeamless != null)
            _toggleSeamless.Click -= ToggleSeamlessOnClick;
        if (_flipStops != null)
            _flipStops.Click -= FlipStopsOnClick;
        if (_rotateStops != null)
            _rotateStops.Click -= RotateStopsOnClick;

        _colorPicker = e.NameScope.Find<ColorPicker>("ColorPicker");
        _gradient = e.NameScope.Find<Border>("Gradient");
        _spreadStops = e.NameScope.Find<Button>("SpreadStops");
        _toggleSeamless = e.NameScope.Find<Button>("ToggleSeamless");
        _flipStops = e.NameScope.Find<Button>("FlipStops");
        _rotateStops = e.NameScope.Find<Button>("RotateStops");

        if (_gradient != null)
            _gradient.PointerPressed += GradientOnPointerPressed;
        if (_spreadStops != null)
            _spreadStops.Click += SpreadStopsOnClick;
        if (_toggleSeamless != null)
            _toggleSeamless.Click += ToggleSeamlessOnClick;
        if (_flipStops != null)
            _flipStops.Click += FlipStopsOnClick;
        if (_rotateStops != null)
            _rotateStops.Click += RotateStopsOnClick;

        base.OnApplyTemplate(e);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Subscribe();

        KeyUp += OnKeyUp;
        KeyDown += OnKeyDown;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        KeyUp -= OnKeyUp;
        KeyDown -= OnKeyDown;

        _shiftDown = false;
    }

    private static void ColorGradientChanged(IAvaloniaObject sender, bool before)
    {
        (sender as GradientPicker)?.Subscribe();
    }

    private static void StorageProviderChanged(IAvaloniaObject sender, bool before)
    {
    }

    private void Subscribe()
    {
        Unsubscribe();

        ColorGradient.CollectionChanged += ColorGradientOnCollectionChanged;
        ColorGradient.StopChanged += ColorGradientOnStopChanged;
        SelectedColorStop = ColorGradient.FirstOrDefault();

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

    private void UpdateGradient()
    {
        // Update the display gradient
        GradientStops collection = new();
        foreach (ColorGradientStop c in ColorGradient.OrderBy(s => s.Position))
            collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));

        LinearGradientBrush.GradientStops = collection;
    }

    private void GradientOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_gradient == null)
            return;

        float position = (float) (e.GetPosition(_gradient).X / _gradient.Bounds.Width);

        ColorGradientStop newStop = new(ColorGradient.GetColor(position), position);
        ColorGradient.Add(newStop);
        SelectedColorStop = newStop;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LeftShift or Key.RightShift)
            _shiftDown = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LeftShift or Key.RightShift)
            _shiftDown = false;

        if (e.Key != Key.Delete || SelectedColorStop == null || ColorGradient.Count <= 2)
            return;

        int index = ColorGradient.IndexOf(SelectedColorStop);
        ColorGradient.Remove(SelectedColorStop);
        if (index > ColorGradient.Count - 1)
            index--;

        SelectedColorStop = ColorGradient.ElementAtOrDefault(index);
        e.Handled = true;
    }

    private void SpreadStopsOnClick(object? sender, RoutedEventArgs e)
    {
        ColorGradient.SpreadStops();
    }

    private void ToggleSeamlessOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || ColorGradient.Count < 2)
            return;

        ColorGradient.ToggleSeamless();
    }

    private void FlipStopsOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || ColorGradient.Count < 2)
            return;

        ColorGradient.FlipStops();
    }

    private void RotateStopsOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || ColorGradient.Count < 2)
            return;

        ColorGradient.RotateStops(_shiftDown);
    }
}