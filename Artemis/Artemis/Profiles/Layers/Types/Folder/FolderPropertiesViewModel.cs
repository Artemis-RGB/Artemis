using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Folder
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