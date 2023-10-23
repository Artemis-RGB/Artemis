using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;

public partial class LayoutInfoStepView : ReactiveUserControl<LayoutInfoStepViewModel>
{
    public LayoutInfoStepView()
    {
        InitializeComponent();
    }
}