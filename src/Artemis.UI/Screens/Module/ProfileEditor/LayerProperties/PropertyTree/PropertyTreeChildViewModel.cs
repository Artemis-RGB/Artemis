using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeChildViewModel : PropertyTreeItemViewModel
    {
        private bool _keyframesEnabled;

        public PropertyTreeChildViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
            PropertyInputViewModel = layerPropertyViewModel.GetPropertyInputViewModel();
        }

        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public PropertyInputViewModel PropertyInputViewModel { get; set; }

        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                _keyframesEnabled = value;
                UpdateKeyframes();
            }
        }

        private void UpdateKeyframes()
        {
        }
    }
}