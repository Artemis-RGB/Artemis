using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreeViewModel : Screen
    {
        public TreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups)
        {
            LayerPropertiesViewModel = layerPropertiesViewModel;

            // Not using the Items collection because the list should persist even after this VM gets closed
            LayerPropertyGroups = layerPropertyGroups;
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public IObservableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }

        public void PropertyTreePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled || !(sender is System.Windows.Controls.TreeView))
                return;

            e.Handled = true;
            MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
                                           {
                                               RoutedEvent = UIElement.MouseWheelEvent,
                                               Source = sender
                                           };
            UIElement parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
