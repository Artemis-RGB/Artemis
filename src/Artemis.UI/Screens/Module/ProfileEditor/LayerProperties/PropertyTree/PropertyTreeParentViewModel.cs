using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeParentViewModel : PropertyTreeItemViewModel
    {
        public PropertyTreeParentViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            Children = new BindableCollection<PropertyTreeItemViewModel>();
        }

        public BindableCollection<PropertyTreeItemViewModel> Children { get; set; }

        public override void Update(bool forceUpdate)
        {
            foreach (var child in Children)
                child.Update(forceUpdate);
        }

        public override void AddLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            if (layerPropertyViewModel.Parent == LayerPropertyViewModel)
            {
                lock (Children)
                {
                    var index = layerPropertyViewModel.LayerProperty.Parent.Children.IndexOf(layerPropertyViewModel.LayerProperty);
                    if (index > Children.Count)
                        index = Children.Count;
                    if (layerPropertyViewModel.Children.Any())
                        Children.Insert(index, new PropertyTreeParentViewModel(layerPropertyViewModel));
                    else
                        Children.Insert(index, new PropertyTreeChildViewModel(layerPropertyViewModel));
                }
            }
            else
            {
                foreach (var propertyTreeItemViewModel in Children)
                    propertyTreeItemViewModel.AddLayerProperty(layerPropertyViewModel);
            }
        }

        public override void RemoveLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            foreach (var child in Children.ToList())
            {
                if (child.LayerPropertyViewModel == layerPropertyViewModel)
                    Children.Remove(child);
                else
                    child.RemoveLayerProperty(layerPropertyViewModel);
            }
        }
    }
}