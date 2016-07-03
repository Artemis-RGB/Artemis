using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Layers
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        private LayerModel _layerModel;

        protected LayerPropertiesViewModel(LayerModel layerModel, IDataModel dataModel)
        {
            LayerModel = layerModel;
            DataModel = dataModel;
        }

        public LayerModel LayerModel
        {
            get { return _layerModel; }
            set
            {
                if (Equals(value, _layerModel)) return;
                _layerModel = value;
                NotifyOfPropertyChange(() => LayerModel);
            }
        }

        public IDataModel DataModel { get; set; }

        public abstract void ApplyProperties();
    }
}