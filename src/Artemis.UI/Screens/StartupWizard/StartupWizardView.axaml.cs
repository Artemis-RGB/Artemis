using System;
using System.Reactive.Disposables;
using Artemis.UI.Screens.StartupWizard.Steps;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;
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