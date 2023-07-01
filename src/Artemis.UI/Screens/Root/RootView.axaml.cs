using System;
using System.Reactive.Disposables;
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

    private void Navigate(IMainScreenViewModel viewModel)
    {
        try
        {
            Dispatcher.UIThread.Invoke(() => RootFrame.NavigateFromObject(viewModel));
        }
        catch (Exception)
        {
            // ignored
        }
    }
}