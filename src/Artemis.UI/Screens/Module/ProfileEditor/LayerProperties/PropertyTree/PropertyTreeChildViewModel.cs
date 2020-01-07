using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeChildViewModel : PropertyTreeItemViewModel
    {
        private bool _keyframesEnabled;

        public PropertyTreeChildViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;

            // TODO: Leverage DI for this and make it less shitty :))
            if (LayerPropertyViewModel.LayerProperty.Type == typeof(SKPoint))
                PropertyInputViewModel = new SKPointPropertyInputViewModel(LayerPropertyViewModel);
            else if (LayerPropertyViewModel.LayerProperty.Type == typeof(SKSize))
                PropertyInputViewModel = new SKSizePropertyInputViewModel(LayerPropertyViewModel);
            else if (LayerPropertyViewModel.LayerProperty.Type == typeof(int))
                PropertyInputViewModel = new IntPropertyInputViewModel(LayerPropertyViewModel);   
            else if (LayerPropertyViewModel.LayerProperty.Type == typeof(float))
                PropertyInputViewModel = new FloatPropertyInputViewModel(LayerPropertyViewModel);
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