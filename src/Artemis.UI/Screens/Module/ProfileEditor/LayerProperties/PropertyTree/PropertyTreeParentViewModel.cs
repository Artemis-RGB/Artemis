using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeParentViewModel : PropertyTreeItemViewModel
    {
        public PropertyTreeParentViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
            Children = new BindableCollection<PropertyTreeItemViewModel>();

            foreach (var childProperty in layerPropertyViewModel.Children)
            {
                if (childProperty.Children.Any())
                    Children.Add(new PropertyTreeParentViewModel(childProperty));
                else
                    Children.Add(new PropertyTreeChildViewModel(childProperty));
            }
        }

        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTreeItemViewModel> Children { get; set; }
    }
}