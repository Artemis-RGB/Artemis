using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Caliburn.Micro;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Profiles.Layers.Abstract
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        private LayerModel _layerModel;
        private Brush _brush;

        protected LayerPropertiesViewModel(LayerModel layerModel, IDataModel dataModel)
        {
            LayerModel = layerModel;
            DataModel = dataModel;
            Brush = LayerModel.Properties.Brush.Clone();
        }

        public Brush Brush
        {
            get { return _brush; }
            set
            {
                if (Equals(value, _brush)) return;
                _brush = value;
                NotifyOfPropertyChange(() => Brush);
            }
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