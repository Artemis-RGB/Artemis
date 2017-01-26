using System.Windows.Media;
using Artemis.Profiles.Layers.Models;
using Artemis.ViewModels;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Abstract
{
    public abstract class LayerPropertiesViewModel : PropertyChangedBase
    {
        private Brush _brush;
        private LayerModel _layerModel;

        protected LayerPropertiesViewModel(LayerEditorViewModel layerEditorViewModel)
        {
            LayerEditorViewModel = layerEditorViewModel;
            LayerModel = layerEditorViewModel.ProposedLayer;

            Brush = LayerModel.Properties.Brush.Clone();
        }

        public LayerEditorViewModel LayerEditorViewModel { get; set; }

        /// <summary>
        ///     The proposed brush
        /// </summary>
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

        /// <summary>
        ///     The proposed layer
        /// </summary>
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

        public abstract void ApplyProperties();
    }
}