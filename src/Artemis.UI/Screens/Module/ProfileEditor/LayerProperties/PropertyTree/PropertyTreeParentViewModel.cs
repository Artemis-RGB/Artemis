using System.Linq;
using Artemis.Core.Events;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeParentViewModel : PropertyTreeItemViewModel
    {
        public PropertyTreeParentViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            Children = new BindableCollection<PropertyTreeItemViewModel>();

            foreach (var childProperty in layerPropertyViewModel.Children)
            {
                if (childProperty.Children.Any())
                    Children.Add(new PropertyTreeParentViewModel(childProperty));
                else
                    Children.Add(new PropertyTreeChildViewModel(childProperty));
            }

            LayerPropertyViewModel.LayerProperty.Layer.LayerPropertyRegistered += LayerOnLayerPropertyRegistered;
            LayerPropertyViewModel.LayerProperty.Layer.LayerPropertyRemoved += LayerOnLayerPropertyRemoved;
        }

        private void LayerOnLayerPropertyRegistered(object sender, LayerPropertyEventArgs e)
        {
            if (e.LayerProperty.Parent == LayerPropertyViewModel.LayerProperty)
            {
                // Problem is we don't have a LayerPropertyViewModel here..
            }
        }

        private void LayerOnLayerPropertyRemoved(object sender, LayerPropertyEventArgs e)
        {
            // Remove self
            if (e.LayerProperty == LayerPropertyViewModel.LayerProperty)
            {
                LayerPropertyViewModel.LayerProperty.Layer.LayerPropertyRemoved -= LayerOnLayerPropertyRegistered;
                LayerPropertyViewModel.LayerProperty.Layer.LayerPropertyRemoved -= LayerOnLayerPropertyRemoved;
            }

            // Remove child
            if (e.LayerProperty.Parent == LayerPropertyViewModel.LayerProperty)
            {
                var child = Children.FirstOrDefault(c => c.LayerPropertyViewModel.LayerProperty == e.LayerProperty);
                if (child != null)
                    Children.Remove(child);
            }
        }

        public BindableCollection<PropertyTreeItemViewModel> Children { get; set; }

        public override void Update(bool forceUpdate)
        {
            foreach (var child in Children)
                child.Update(forceUpdate);
        }
    }
}