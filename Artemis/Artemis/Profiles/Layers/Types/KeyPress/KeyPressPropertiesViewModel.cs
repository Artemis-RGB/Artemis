using Artemis.Profiles.Layers.Abstract;
using Artemis.ViewModels;
using Artemis.ViewModels.Profiles;

namespace Artemis.Profiles.Layers.Types.KeyPress
{
    public class KeyPressPropertiesViewModel : LayerPropertiesViewModel
    {
        public KeyPressPropertiesViewModel(LayerEditorViewModel editorVm) : base(editorVm)
        {
        }

        public override void ApplyProperties()
        {
            LayerModel.Properties.Brush = Brush;
        }
    }
}