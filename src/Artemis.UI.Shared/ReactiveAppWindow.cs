using System;
using System.Reactive.Disposables.Fluent;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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

    private bool _micaEnabled;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReactiveAppWindow{TViewModel}" /> class.
    /// </summary>
    public ReactiveAppWindow()
    {
        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.
        this.WhenActivated(disposables => UI.MicaEnabled.Subscribe(ToggleMica).DisposeWith(disposables));
        this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);
    }

    private void ToggleMica(bool enable)
    {
        if (enable == _micaEnabled)
            return;

        if (enable)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !IsWindows11)
                return;

            // TransparencyBackgroundFallback = Brushes.Transparent;
            TransparencyLevelHint = new[] {WindowTransparencyLevel.Mica};
            Background = new SolidColorBrush(new Color(80, 0,0,0));

        }
        else
        {
            ClearValue(TransparencyLevelHintProperty);
            ClearValue(BackgroundProperty);
        }

        _micaEnabled = enable;
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