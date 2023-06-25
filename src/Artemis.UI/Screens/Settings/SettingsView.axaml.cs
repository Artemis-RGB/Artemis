using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
        this.WhenActivated(d => { ViewModel.WhenAnyValue(vm => vm.CurrentScreen).WhereNotNull().Subscribe(Navigate).DisposeWith(d); });
    }

    private void Navigate(ViewModelBase viewModel)
    {
        Dispatcher.UIThread.Invoke(() => TabFrame.NavigateFromObject(viewModel));
    }
}