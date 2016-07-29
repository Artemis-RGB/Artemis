using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.KeyPress
{
    public class KeyPressPropertiesViewModel : LayerPropertiesViewModel
    {
        public KeyPressPropertiesViewModel(LayerModel layerModel, IDataModel dataModel) : base(layerModel, dataModel)
        {
        }

        public override void ApplyProperties()
        {
            LayerModel.Properties.Brush = Brush;
        }
    }
}