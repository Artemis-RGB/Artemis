using System.Linq;
using System.Windows.Forms;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Utilities;
using Artemis.ViewModels;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.Keyboard
{
    public class KeyboardPropertiesViewModel : LayerPropertiesViewModel
    {
        private bool _isGif;
        private ILayerAnimation _selectedLayerAnimation;

        public KeyboardPropertiesViewModel(LayerEditorViewModel editorVm) : base(editorVm)
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

        public bool ShowGif => IsGif;
        public bool ShowBrush => !IsGif;
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }
        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }
        public LayerTweenViewModel LayerTweenViewModel { get; set; }

        public bool IsGif
        {
            get { return _isGif; }
            set
            {
                _isGif = value;
                NotifyOfPropertyChange(() => ShowGif);
                NotifyOfPropertyChange(() => ShowBrush);
            }
        }

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

        public void BrowseGif()
        {
            var dialog = new OpenFileDialog {Filter = "Animated image file (*.gif)|*.gif"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((KeyboardPropertiesModel) LayerModel.Properties).GifFile = dialog.FileName;
            NotifyOfPropertyChange(() => LayerModel);
        }

        public override void ApplyProperties()
        {
            HeightProperties.Apply(LayerModel);
            WidthProperties.Apply(LayerModel);
            OpacityProperties.Apply(LayerModel);
            LayerModel.Properties.Brush = Brush;

            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }
    }
}