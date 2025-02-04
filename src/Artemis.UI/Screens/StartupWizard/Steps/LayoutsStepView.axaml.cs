using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class LayoutsStepView : ReactiveUserControl<LayoutsStepViewModel>
{
    public LayoutsStepView()
    {
        InitializeComponent();
    }
}