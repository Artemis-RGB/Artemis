using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class ReleasesTabView : ReactiveUserControl<ReleasesTabViewModel>
{
    public ReleasesTabView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d));
    }

    private void Navigate(ViewModelBase viewModel)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            try
            {
                ReleaseFrame.NavigateFromObject(viewModel);
            }
            catch (Exception e)
            {
                // ignored
            }
        });
    }

}