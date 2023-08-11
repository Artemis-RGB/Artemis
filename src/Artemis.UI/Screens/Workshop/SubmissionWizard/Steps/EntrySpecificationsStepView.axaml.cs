using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class EntrySpecificationsStepView : ReactiveUserControl<EntrySpecificationsStepViewModel>
{
    public EntrySpecificationsStepView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}