using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class WelcomeStepView : ReactiveUserControl<WelcomeStepViewModel>
{
    public WelcomeStepView()
    {
        InitializeComponent();
    }

}