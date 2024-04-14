using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
        this.WhenActivated(d => { ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d); });
    }

    private void Navigate(ViewModelBase viewModel)
    {
        TabFrame.NavigateFromObject(viewModel);
    }
 
    private void NavigationView_OnBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        ViewModel?.GoBack();
    }
}