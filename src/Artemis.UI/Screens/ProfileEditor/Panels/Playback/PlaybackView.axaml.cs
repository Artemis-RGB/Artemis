using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Playback
{
    public partial class PlaybackView : ReactiveUserControl<PlaybackViewModel>
    {
        public PlaybackView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
