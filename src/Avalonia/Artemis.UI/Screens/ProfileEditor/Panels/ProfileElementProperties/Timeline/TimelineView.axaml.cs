using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline
{
    public partial class TimelineView : ReactiveUserControl<TimelineViewModel>
    {
        public TimelineView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
