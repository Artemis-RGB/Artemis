using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class UploadStepView : ReactiveUserControl<UploadStepViewModel>
{
    public UploadStepView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}