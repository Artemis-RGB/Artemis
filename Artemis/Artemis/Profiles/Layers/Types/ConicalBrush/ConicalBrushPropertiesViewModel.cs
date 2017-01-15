using System.Linq;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Utilities;
using Artemis.ViewModels;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.ConicalBrush
{
    public class ConicalBrushPropertiesViewModel : LayerPropertiesViewModel
    {
        #region Properties & Fields

        private ILayerAnimation _selectedLayerAnimation;

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }
        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }
        public LayerTweenViewModel LayerTweenViewModel { get; set; }

        public ILayerAnimation SelectedLayerAnimation
        {
            get { return _selectedLayerAnimation; }
            set
            {
                if (Equals(value, _selectedLayerAnimation)) return;
                _selectedLayerAnimation = value;
                NotifyOfPropertyChange(() => SelectedLayerAnimation);
            }
        }

        #endregion

        #region Constructors

        public ConicalBrushPropertiesViewModel(LayerEditorViewModel editorVm)
            : base(editorVm)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(editorVm.LayerAnimations);

            HeightProperties = new LayerDynamicPropertiesViewModel("Height", editorVm);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", editorVm);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", editorVm);
            LayerTweenViewModel = new LayerTweenViewModel(editorVm);

            SelectedLayerAnimation =
                LayerAnimations.FirstOrDefault(l => l.Name == editorVm.ProposedLayer.LayerAnimation?.Name) ??
                LayerAnimations.First(l => l.Name == "None");
        }

        #endregion

        #region Methods

        public override void ApplyProperties()
        {
            HeightProperties.Apply(LayerModel);
            WidthProperties.Apply(LayerModel);
            OpacityProperties.Apply(LayerModel);

            LayerModel.Properties.Brush = Brush;
            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }

        #endregion
    }
}
