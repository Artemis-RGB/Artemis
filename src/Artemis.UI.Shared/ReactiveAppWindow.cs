using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;
using ReactiveUI;

namespace Artemis.UI.Shared;

/// <summary>
///     A ReactiveUI <see cref="Window" /> that implements the <see cref="IViewFor{TViewModel}" /> interface and will
///     activate your ViewModel automatically if the view model implements <see cref="IActivatableViewModel" />. When
///     the DataContext property changes, this class will update the ViewModel property with the new DataContext value,
///     and vice versa.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
public class ReactiveAppWindow<TViewModel> : AppWindow, IViewFor<TViewModel> where TViewModel : class
{
    /// <summary>
    ///     The ViewModel.
    /// </summary>
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReactiveAppWindow{TViewModel}" /> class.
    /// </summary>
    public ReactiveAppWindow()
    {
        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.
        this.WhenActivated(disposables => { });
        this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);
    }

    /// <inheritdoc />
    protected override void OnOpened(EventArgs e)
    {
        // TODO: Move to a style and remove opacity on focus loss
        base.OnOpened(e);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !IsWindows11)
            return;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;
        TryEnableMicaEffect();
    }

    private void OnDataContextChanged(object? value)
    {
        if (value is TViewModel viewModel)
            ViewModel = viewModel;
        else
            ViewModel = null;
    }

    private void OnViewModelChanged(object? value)
    {
        if (value == null)
            ClearValue(DataContextProperty);
        else if (DataContext != value) DataContext = value;
    }

    private void TryEnableMicaEffect()
    {
        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            Color2 color = this.TryFindResource("SolidBackgroundFillColorBase", ThemeVariant.Dark, out object? value) ? (Color) value : new Color2(32, 32, 32);
            color = color.LightenPercent(-0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.78);
        }
        else if (ActualThemeVariant == ThemeVariant.Light)
        {
            // Similar effect here
            Color2 color = this.TryFindResource("SolidBackgroundFillColorBase", ThemeVariant.Light, out object? value) ? (Color) value : new Color2(243, 243, 243);
            color = color.LightenPercent(0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }

    /// <summary>
    ///     The ViewModel.
    /// </summary>
    public TViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?) value;
    }
}