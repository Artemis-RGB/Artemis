using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class WorkshopUnreachableStepView : ReactiveUserControl<WorkshopUnreachableStepViewModel>
{
    public WorkshopUnreachableStepView()
    {
        InitializeComponent();
    }
}