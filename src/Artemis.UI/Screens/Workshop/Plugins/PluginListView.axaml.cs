using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginListView : ReactiveUserControl<PluginListViewModel>
{
    public PluginListView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen ?? ViewModel?.EntryListViewModel))
            .DisposeWith(d));
    }
}