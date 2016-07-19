using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Abstract
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        private Brush _brush;
        private LayerModel _layerModel;

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