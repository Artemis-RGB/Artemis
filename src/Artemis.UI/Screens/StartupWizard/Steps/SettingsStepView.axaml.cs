using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class SettingsStepView : ReactiveUserControl<SettingsStepViewModel>
{
    public SettingsStepView()
    {
        InitializeComponent();
    }

}