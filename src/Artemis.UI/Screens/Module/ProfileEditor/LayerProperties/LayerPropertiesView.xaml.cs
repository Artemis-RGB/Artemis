using System.Windows.Controls;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
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
            if (sender == TimelineHeaderScrollViewer)
            {
                TimelineRailsScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else if (sender == TimelineRailsScrollViewer)
            {
                TimelineHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }
    }
}