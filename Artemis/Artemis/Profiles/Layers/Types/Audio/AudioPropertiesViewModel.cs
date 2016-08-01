using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesViewModel : LayerPropertiesViewModel
    {
        public AudioPropertiesViewModel(LayerModel layerModel, IDataModel dataModel) : base(layerModel, dataModel)
        {
        }

        public override void ApplyProperties()
        {
            LayerModel.Properties.Brush = Brush;
        }
    }
}