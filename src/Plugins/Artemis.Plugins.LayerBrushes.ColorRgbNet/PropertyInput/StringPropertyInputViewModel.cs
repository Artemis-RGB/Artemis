using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet.PropertyInput
{
    public class StringPropertyInputViewModel : PropertyInputViewModel<string>
    {
        public StringPropertyInputViewModel(LayerProperty<string> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
            // This is a fairly dumb input that can only take text and nothing else so it needs no special logic in its VM
        }
    }
}