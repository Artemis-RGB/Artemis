using System;
using System.Reactive.Disposables.Fluent;
using Artemis.UI.Shared;
using ReactiveUI.Avalonia;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class WorkshopLibraryView : ReactiveUserControl<WorkshopLibraryViewModel>
{
    public WorkshopLibraryView()
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