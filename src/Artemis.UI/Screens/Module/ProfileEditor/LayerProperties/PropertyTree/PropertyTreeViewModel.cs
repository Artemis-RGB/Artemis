using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeViewModel : PropertyChangedBase
    {
        public PropertyTreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel)
        {
            LayerPropertiesViewModel = layerPropertiesViewModel;
            PropertyTreeItemViewModels = new BindableCollection<PropertyTreeItemViewModel>();
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public BindableCollection<PropertyTreeItemViewModel> PropertyTreeItemViewModels { get; set; }

        public void PopulateProperties(List<LayerPropertyViewModel> properties)
        {
            PropertyTreeItemViewModels.Clear();

            // Only put parents on the top-level, let parents populate their own children recursively
            foreach (var property in properties)
            {
                if (property.Children.Any())
                    PropertyTreeItemViewModels.Add(new PropertyTreeParentViewModel(property));
            }
        }

        public void ClearProperties()
        {
            PropertyTreeItemViewModels.Clear();
        }
    }
}