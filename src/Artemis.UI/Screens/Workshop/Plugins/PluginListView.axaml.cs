using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Routing;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginListView : ReactiveUserControl<PluginListViewModel>
{
    public PluginListView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.Screen).Subscribe(Navigate).DisposeWith(d);
        });
    }
    
    private void Navigate(RoutableScreen? viewModel)
    {
        RouterFrame.NavigateFromObject(viewModel ?? ViewModel?.EntryListViewModel);
    }
}