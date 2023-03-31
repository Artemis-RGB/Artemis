using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public partial class TimelinePropertyView : ReactiveUserControl<ITimelinePropertyViewModel>
{
    public TimelinePropertyView()
    {
        InitializeComponent();
    }

}