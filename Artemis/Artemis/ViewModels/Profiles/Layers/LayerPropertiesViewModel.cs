using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Layers
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        protected LayerPropertiesViewModel(IDataModel dataModel)
        {
            DataModel = dataModel;
        }

        public IDataModel DataModel { get; set; }

        public abstract LayerPropertiesModel GetAppliedProperties();
    }
}