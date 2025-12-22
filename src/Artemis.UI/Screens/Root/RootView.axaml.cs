using System;
using System.Reactive.Disposables.Fluent;
using Artemis.UI.Shared.Routing;
using ReactiveUI.Avalonia;
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