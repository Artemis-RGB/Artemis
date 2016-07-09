using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.Keyboard
{
    public class KeyboardPropertiesViewModel : LayerPropertiesViewModel
    {
        private bool _isGif;
        private ILayerAnimation _selectedLayerAnimation;

        public KeyboardPropertiesViewModel(LayerModel layerModel, IDataModel dataModel,
            IEnumerable<ILayerAnimation> layerAnimations) : base(layerModel, dataModel)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(layerAnimations);

            var dataModelProps =
                new BindableCollection<GeneralHelpers.PropertyCollection>(GeneralHelpers.GenerateTypeMap(dataModel));
            HeightProperties = new LayerDynamicPropertiesViewModel("Height", dataModelProps, layerModel.Properties);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", dataModelProps, layerModel.Properties);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", dataModelProps, layerModel.Properties);

            SelectedLayerAnimation = LayerAnimations.FirstOrDefault(l => l.Name == layerModel.LayerAnimation?.Name) ??
                                     LayerAnimations.First(l => l.Name == "None");
        }

        public bool ShowGif => IsGif;
        public bool ShowBrush => !IsGif;
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }
        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

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