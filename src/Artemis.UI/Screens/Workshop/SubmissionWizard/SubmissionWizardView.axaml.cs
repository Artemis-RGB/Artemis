using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard;

public partial class SubmissionWizardView : ReactiveAppWindow<SubmissionWizardViewModel>
{
    public SubmissionWizardView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).Subscribe(Navigate).DisposeWith(d));
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