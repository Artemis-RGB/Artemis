using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Properties
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        public IDataModel DataModel { get; set; }

        protected LayerPropertiesViewModel(IDataModel dataModel)
        {
            DataModel = dataModel;
        }

        public abstract LayerPropertiesModel GetAppliedProperties();
    }
}