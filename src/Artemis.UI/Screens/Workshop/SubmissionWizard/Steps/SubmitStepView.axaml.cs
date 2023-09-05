using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class SubmitStepView : ReactiveUserControl<SubmitStepViewModel>
{
    public SubmitStepView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}