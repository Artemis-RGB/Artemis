using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.ViewModels.Profiles.Layers
{
    public class FolderPropertiesViewModel : LayerPropertiesViewModel
    {
        public FolderPropertiesViewModel(LayerModel layerModel, IDataModel dataModel) : base(layerModel, dataModel)
        {
        }

        public override void ApplyProperties()
        {
        }
    }
}