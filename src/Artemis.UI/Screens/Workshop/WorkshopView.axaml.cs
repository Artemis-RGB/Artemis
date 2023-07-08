using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
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