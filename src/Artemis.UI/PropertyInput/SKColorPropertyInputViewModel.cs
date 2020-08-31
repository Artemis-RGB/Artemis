using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using SkiaSharp;

namespace Artemis.UI.PropertyInput
{
    public class SKColorPropertyInputViewModel : PropertyInputViewModel<SKColor>
    {
        public SKColorPropertyInputViewModel(LayerProperty<SKColor> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
        }
    }
}