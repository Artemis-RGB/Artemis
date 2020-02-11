using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeChildViewModel : PropertyTreeItemViewModel
    {
        public PropertyTreeChildViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            PropertyInputViewModel = layerPropertyViewModel.GetPropertyInputViewModel();
        }

        public PropertyInputViewModel PropertyInputViewModel { get; set; }

        public override void Update(bool forceUpdate)
        {
            if (forceUpdate)
                PropertyInputViewModel?.Update();
            else
            {
                // Only update if visible and if keyframes are enabled
                if (LayerPropertyViewModel.Parent.IsExpanded && LayerPropertyViewModel.KeyframesEnabled)
                    PropertyInputViewModel?.Update();
            }
        }
    }
}