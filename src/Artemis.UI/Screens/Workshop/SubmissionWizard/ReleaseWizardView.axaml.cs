using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public partial class ReleaseWizardView: ReactiveAppWindow<ReleaseWizardViewModel>
{
    public ReleaseWizardView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d));
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.ShouldClose).Where(c => c).Subscribe(_ => Close()).DisposeWith(d));
    }

    private void Navigate(SubmissionViewModel viewModel)
    {
        try
        {
            Dispatcher.UIThread.Invoke(() => Frame.NavigateFromObject(viewModel));
        }
        catch (Exception e)
        {
            ViewModel?.WindowService.ShowExceptionDialog("Wizard screen failed to activate", e);
        }
    }
}