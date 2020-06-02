using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
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