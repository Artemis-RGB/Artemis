using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public class BoolPropertyInputViewModel : PropertyInputViewModel<bool>
    {
        public BoolPropertyInputViewModel(LayerProperty<bool> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
        }


        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }
}