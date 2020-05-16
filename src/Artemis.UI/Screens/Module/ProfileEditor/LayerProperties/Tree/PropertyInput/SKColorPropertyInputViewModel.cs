using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class SKColorPropertyInputViewModel : PropertyInputViewModel<SKColor>
    {
        public SKColorPropertyInputViewModel(LayerPropertyViewModel<SKColor> layerPropertyViewModel) : base(layerPropertyViewModel)
        {
        }
    }
}