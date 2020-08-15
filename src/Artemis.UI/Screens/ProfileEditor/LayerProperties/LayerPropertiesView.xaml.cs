using System.Windows.Controls;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    /// <summary>
    ///     Interaction logic for LayerPropertiesView.xaml
    /// </summary>
    public partial class LayerPropertiesView : UserControl
    {
        public LayerPropertiesView()
        {
            InitializeComponent();
        }

        // Keeping the scroll viewers in sync is up to the view, not a viewmodel concern
        private void TimelineScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource == TimelineHeaderScrollViewer)
                TimelineRailsScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            else if (e.OriginalSource == PropertyTreeScrollViewer)
                TimelineRailsScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            else if (e.OriginalSource == TimelineRailsScrollViewer)
            {
                TimelineHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                PropertyTreeScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }
    }
}