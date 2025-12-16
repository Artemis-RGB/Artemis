using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDetailsView : ReactiveUserControl<LayoutDetailsViewModel>
{
    public LayoutDetailsView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .WhereNotNull()
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen))
            .DisposeWith(d));
    }
}