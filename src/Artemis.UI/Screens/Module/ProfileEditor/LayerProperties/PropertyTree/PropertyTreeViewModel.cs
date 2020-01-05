using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeViewModel : PropertyChangedBase
    {
        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public BindableCollection<PropertyTreeChildViewModel> PropertyTreeItemViewModels { get; set; }
        public PropertyTreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel)
        {
            LayerPropertiesViewModel = layerPropertiesViewModel;
        }
    }
}
