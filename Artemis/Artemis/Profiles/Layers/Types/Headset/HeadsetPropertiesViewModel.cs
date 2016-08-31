using System.Linq;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.Headset
{
    public class HeadsetPropertiesViewModel : LayerPropertiesViewModel
    {
        private ILayerAnimation _selectedLayerAnimation;

        public HeadsetPropertiesViewModel(LayerEditorViewModel editorVm) : base(editorVm)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(editorVm.LayerAnimations);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", editorVm);

            SelectedLayerAnimation =
                LayerAnimations.FirstOrDefault(l => l.Name == editorVm.ProposedLayer.LayerAnimation?.Name) ??
                LayerAnimations.First(l => l.Name == "None");
        }

        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

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

        public override void ApplyProperties()
        {
            OpacityProperties.Apply(LayerModel);
            LayerModel.Properties.Brush = Brush;
            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }
    }
}