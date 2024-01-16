using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class ImagesStepView : ReactiveUserControl<ImagesStepViewModel>
{
    public ImagesStepView()
    {
        InitializeComponent();
    }
}