using System;
using System.Reactive.Disposables.Fluent;
using Artemis.UI.Shared;
using Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public partial class StartupWizardView : ReactiveAppWindow<StartupWizardViewModel>
{
    public StartupWizardView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d));
    }


    private void Navigate(WizardStepViewModel viewModel)
    {
        try
        {
            Frame.NavigateFromObject(viewModel);
        }
        catch (Exception e)
        {
            ViewModel?.WindowService.ShowExceptionDialog("Wizard screen failed to activate", e);
        }
    }
}