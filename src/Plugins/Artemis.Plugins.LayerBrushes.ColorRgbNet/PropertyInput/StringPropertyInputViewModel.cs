using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

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