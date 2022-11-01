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
///     Represents a gradient picker that can be used to edit a gradient.
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

    /// <summary>
    /// Gets the color gradient currently being edited, for internal use only
    /// </summary>
    public static readonly DirectProperty<GradientPicker, ColorGradient> EditingColorGradientProperty =
        AvaloniaProperty.RegisterDirect<GradientPicker, ColorGradient>(nameof(EditingColorGradient), g => g.EditingColorGradient);

    private readonly ICommand _deleteStop;
    private ColorPicker? _colorPicker;
    private Button? _flipStops;
    private Border? _gradient;
    private Button? _randomize;
    private Button? _rotateStops;
    private bool _shiftDown;
    private Button? _spreadStops;
    private Button? _toggleSeamless;
    private ColorGradient _colorGradient = null!;

    /// <summary>
    ///     Creates a new instance of the <see cref="GradientPicker" /> class.
    /// </summary>
    public GradientPicker()
    {
        EditingColorGradient = ColorGradient.GetUnicornBarf();
        _deleteStop = ReactiveCommand.Create<ColorGradientStop>(s =>
        {
            if (ColorGradient.Count <= 2)
                return;

            int index = EditingColorGradient.IndexOf(s);
            EditingColorGradient.Remove(s);
            if (index > EditingColorGradient.Count - 1)
                index--;

            SelectedColorStop = EditingColorGradient.ElementAtOrDefault(index);
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
    
    /// <summary>
    /// Gets the color gradient backing the editor, this is a copy of <see cref="ColorGradient"/>.
    /// </summary>
    public ColorGradient EditingColorGradient
    {
        get => _colorGradient;
        private set => SetAndRaise(EditingColorGradientProperty, ref _colorGradient, value);
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
        if (_randomize != null)
            _randomize.Click -= RandomizeOnClick;

        _colorPicker = e.NameScope.Find<ColorPicker>("ColorPicker");
        _gradient = e.NameScope.Find<Border>("Gradient");
        _spreadStops = e.NameScope.Find<Button>("SpreadStops");
        _toggleSeamless = e.NameScope.Find<Button>("ToggleSeamless");
        _flipStops = e.NameScope.Find<Button>("FlipStops");
        _rotateStops = e.NameScope.Find<Button>("RotateStops");
        _randomize = e.NameScope.Find<Button>("Randomize");
        
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
        if (_randomize != null)
            _randomize.Click += RandomizeOnClick;

        base.OnApplyTemplate(e);
    }


    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ApplyToField();

        KeyUp += OnKeyUp;
        KeyDown += OnKeyDown;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ApplyToProperty();
        KeyUp -= OnKeyUp;
        KeyDown -= OnKeyDown;

        _shiftDown = false;
    }


    private static void ColorGradientChanged(IAvaloniaObject sender, bool before)
    {
        (sender as GradientPicker)?.ApplyToField();
    }

    private static void StorageProviderChanged(IAvaloniaObject sender, bool before)
    {
    }

    private void ApplyToField()
    {
        EditingColorGradient = new ColorGradient(ColorGradient);
        EditingColorGradient.CollectionChanged += ColorGradientOnCollectionChanged;
        EditingColorGradient.StopChanged += ColorGradientOnStopChanged;
        SelectedColorStop = EditingColorGradient.FirstOrDefault();
        UpdateGradient();
    }

    private void ApplyToProperty()
    {
        // Remove extra color gradients
        while (ColorGradient.Count > EditingColorGradient.Count)
            ColorGradient.RemoveAt(ColorGradient.Count - 1);

        for (int index = 0; index < EditingColorGradient.Count; index++)
        {
            ColorGradientStop colorGradientStop = EditingColorGradient[index];
            // Add missing color gradients
            if (index >= ColorGradient.Count)
            {
                ColorGradient.Add(new ColorGradientStop(colorGradientStop.Color, colorGradientStop.Position));
            }
            // Update existing color gradients
            else
            {
                ColorGradient[index].Color = colorGradientStop.Color;
                ColorGradient[index].Position = colorGradientStop.Position;
            }
        }
    }

    private void ColorGradientOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateGradient();
        ApplyToProperty();
    }

    private void ColorGradientOnStopChanged(object? sender, EventArgs e)
    {
        UpdateGradient();
        ApplyToProperty();
    }

    private void UpdateGradient()
    {
        // Update the display gradient
        GradientStops collection = new();
        foreach (ColorGradientStop c in EditingColorGradient.OrderBy(s => s.Position))
            collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));

        LinearGradientBrush.GradientStops = collection;
    }

    private void GradientOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_gradient == null)
            return;

        float position = (float) (e.GetPosition(_gradient).X / _gradient.Bounds.Width);

        ColorGradientStop newStop = new(EditingColorGradient.GetColor(position), position);
        EditingColorGradient.Add(newStop);
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

        if (e.Key != Key.Delete || SelectedColorStop == null || EditingColorGradient.Count <= 2)
            return;
        
        DeleteStop.Execute(SelectedColorStop);
        e.Handled = true;
    }

    private void SpreadStopsOnClick(object? sender, RoutedEventArgs e)
    {
        EditingColorGradient.SpreadStops();
    }

    private void ToggleSeamlessOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || EditingColorGradient.Count < 2)
            return;

        EditingColorGradient.ToggleSeamless();
    }

    private void FlipStopsOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || EditingColorGradient.Count < 2)
            return;

        EditingColorGradient.FlipStops();
    }

    private void RotateStopsOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedColorStop == null || EditingColorGradient.Count < 2)
            return;

        EditingColorGradient.RotateStops(_shiftDown);
    }

    private void RandomizeOnClick(object? sender, RoutedEventArgs e)
    {
        EditingColorGradient.Randomize(6);
        SelectedColorStop = EditingColorGradient.First();
    }
}