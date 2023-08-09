using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class ProfileSelectionStepView : UserControl
{
    public ProfileSelectionStepView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}