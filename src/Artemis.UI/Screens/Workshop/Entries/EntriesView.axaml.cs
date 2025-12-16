using Artemis.UI.Shared;
using ReactiveUI.Avalonia;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Reactive.Disposables.Fluent;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntriesView : ReactiveUserControl<EntriesViewModel>
{
    public EntriesView()
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