using System;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Artemis.UI.Shared.Controls.GradientPicker;

public class GradientPicker : TemplatedControl
{
    private LinearGradientBrush _linearGradientBrush = new();

    /// <summary>
    ///     Gets or sets the color gradient.
    /// </summary>
    public static readonly StyledProperty<ColorGradient> ColorGradientProperty =
        AvaloniaProperty.Register<GradientPicker, ColorGradient>(nameof(Core.ColorGradient), notifying: ColorGradientChanged, defaultValue: ColorGradient.GetUnicornBarf());

    /// <summary>
    ///     Gets or sets the currently selected color stop.
    /// </summary>
    public static readonly StyledProperty<ColorGradientStop?> SelectedColorStopProperty =
        AvaloniaProperty.Register<GradientPicker, ColorGradientStop?>(nameof(SelectedColorStop), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public static readonly DirectProperty<GradientPicker, LinearGradientBrush> LinearGradientBrushProperty =
        AvaloniaProperty.RegisterDirect<GradientPicker, LinearGradientBrush>(nameof(LinearGradientBrush), g => g.LinearGradientBrush);

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
        set => SetValue(SelectedColorStopProperty, value);
    }

    /// <summary>
    /// Gets the linear gradient brush representing the color gradient.
    /// </summary>
    public LinearGradientBrush LinearGradientBrush
    {
        get => _linearGradientBrush;
        private set => SetAndRaise(LinearGradientBrushProperty, ref _linearGradientBrush, value);
    }

    private static void ColorGradientChanged(IAvaloniaObject sender, bool before)
    {
        if (before)
            (sender as GradientPicker)?.Unsubscribe();
        else
            (sender as GradientPicker)?.Subscribe();
    }

    private void Subscribe()
    {
        ColorGradient.CollectionChanged += ColorGradientOnCollectionChanged;
        ColorGradient.StopChanged += ColorGradientOnStopChanged;

        UpdateGradient();
        SelectedColorStop = ColorGradient.FirstOrDefault();
    }

    private void Unsubscribe()
    {
        ColorGradient.CollectionChanged -= ColorGradientOnCollectionChanged;
        ColorGradient.StopChanged -= ColorGradientOnStopChanged;
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
        // Remove old stops

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (ColorGradient == null)
            return;

        // Add new stops

        // Update the display gradient
        GradientStops collection = new();
        foreach (ColorGradientStop c in ColorGradient.OrderBy(s => s.Position))
            collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));
        LinearGradientBrush = new LinearGradientBrush {GradientStops = collection};
    }

    private void SelectColorStop(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is IDataContextProvider dataContextProvider && dataContextProvider.DataContext is ColorGradientStop colorStop)
            SelectedColorStop = colorStop;
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Subscribe();
        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        base.OnDetachedFromVisualTree(e);
    }

    #endregion
}