using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDetailsView : ReactiveUserControl<PluginDetailsViewModel>
{
    public PluginDetailsView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .WhereNotNull()
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen))
            .DisposeWith(d));
    }
}