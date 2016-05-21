using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor.Properties
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        public IGameDataModel GameDataModel { get; set; }

        protected LayerPropertiesViewModel(IGameDataModel gameDataModel)
        {
            GameDataModel = gameDataModel;
        }

        public abstract LayerPropertiesModel GetAppliedProperties();
    }
}