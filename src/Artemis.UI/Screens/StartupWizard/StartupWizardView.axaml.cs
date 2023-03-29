using System;
using System.Reactive.Disposables;
using Artemis.UI.Screens.StartupWizard.Steps;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public class StartupWizardView : ReactiveAppWindow<StartupWizardViewModel>
{
    private readonly Frame _frame;

    public StartupWizardView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        _frame = this.Get<Frame>("Frame");

        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.CurrentStep).Subscribe(ApplyCurrentStep).DisposeWith(d));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ApplyCurrentStep(int step)
    {
        if (step == 1)
            _frame.NavigateToType(typeof(WelcomeStep), null, new FrameNavigationOptions());
        else if (step == 2)
            _frame.NavigateToType(typeof(DevicesStep), null, new FrameNavigationOptions());
        else if (step == 3)
            _frame.NavigateToType(typeof(LayoutStep), null, new FrameNavigationOptions());
        else if (step == 4)
            _frame.NavigateToType(typeof(SettingsStep), null, new FrameNavigationOptions());
        else if (step == 5)
            _frame.NavigateToType(typeof(FinishStep), null, new FrameNavigationOptions());
    }
}