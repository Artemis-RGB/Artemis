using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop;

public partial class WorkshopView : ReactiveUserControl<WorkshopViewModel>
{
    public WorkshopView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).Subscribe(vm => WorkshopFrame.NavigateFromObject(vm ?? ViewModel?.HomeViewModel)).DisposeWith(d));
    }
}