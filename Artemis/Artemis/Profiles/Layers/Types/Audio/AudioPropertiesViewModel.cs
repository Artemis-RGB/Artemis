using Artemis.Profiles.Layers.Abstract;
using Artemis.ViewModels.Profiles;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesViewModel : LayerPropertiesViewModel
    {
        public AudioPropertiesViewModel(LayerEditorViewModel editorVm) : base(editorVm)
        {
        }

        public override void ApplyProperties()
        {
            LayerModel.Properties.Brush = Brush;
        }
    }
}