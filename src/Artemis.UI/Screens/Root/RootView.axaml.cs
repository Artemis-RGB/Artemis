using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Routing;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root;

public partial class RootView : ReactiveUserControl<RootViewModel>
{
    public RootView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).Subscribe(Navigate).DisposeWith(d));
    }

    private void Navigate(RoutableScreen? viewModel)
    {
        try
        {
            RootFrame.NavigateFromObject(viewModel);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}