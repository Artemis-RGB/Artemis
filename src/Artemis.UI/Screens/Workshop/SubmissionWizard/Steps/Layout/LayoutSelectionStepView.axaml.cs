using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;

public partial class LayoutSelectionStepView : ReactiveUserControl<LayoutSelectionStepViewModel>
{
    public LayoutSelectionStepView()
    {
        InitializeComponent();
    }
}