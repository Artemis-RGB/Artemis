using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root;

public partial class RootView : ReactiveUserControl<RootViewModel>
{
    public RootView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.CurrentScreen).WhereNotNull().Subscribe(TryNavigate).DisposeWith(d);
        });
    }

    private void TryNavigate(ViewModelBase viewModel)
    {
        try
        {
            Dispatcher.UIThread.Invoke(() => RootFrame.NavigateFromObject(viewModel));
        }
        catch (Exception e)
        {
            Console.WriteLine();
        }
    }
}