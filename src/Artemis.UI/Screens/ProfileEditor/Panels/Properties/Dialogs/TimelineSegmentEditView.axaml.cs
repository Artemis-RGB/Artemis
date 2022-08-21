using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;

public class TimelineSegmentEditView : ReactiveUserControl<TimelineSegmentEditViewModel>
{
    public TimelineSegmentEditView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}