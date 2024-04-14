using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Routing;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutListView : ReactiveUserControl<LayoutListViewModel>
{
    public LayoutListView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .WhereNotNull()
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen))
            .DisposeWith(d));
    }
}