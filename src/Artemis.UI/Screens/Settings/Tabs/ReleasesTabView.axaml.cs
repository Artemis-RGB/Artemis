using System;
using System.Reactive.Disposables.Fluent;
using Artemis.UI.Shared;
using ReactiveUI.Avalonia;
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
        try
        {
            ReleaseFrame.NavigateFromObject(viewModel);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}