using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Layers
{
    public class KeyboardPropertiesViewModel : LayerPropertiesViewModel
    {
        private Brush _brush;
        private bool _isGif;
        private KeyboardPropertiesModel _proposedProperties;
        private ILayerAnimation _selectedLayerAnimation;

        public KeyboardPropertiesViewModel(IEnumerable<ILayerAnimation> layerAnimations, IDataModel dataModel,
            LayerModel layerModel)
            : base(dataModel)
        {
            var keyboardProperties = (KeyboardPropertiesModel)layerModel.Properties;
            ProposedProperties = GeneralHelpers.Clone(keyboardProperties);
            Brush = ProposedProperties.Brush.CloneCurrentValue();

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>(
                GeneralHelpers.GenerateTypeMap(dataModel));
            LayerAnimations = new BindableCollection<ILayerAnimation>(layerAnimations);

            HeightProperties = new LayerDynamicPropertiesViewModel("Height", DataModelProps, keyboardProperties);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", DataModelProps, keyboardProperties);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", DataModelProps, keyboardProperties);

            SelectedLayerAnimation = LayerAnimations.FirstOrDefault(l => l.Name == layerModel.LayerType.Name);
        }

        public bool ShowGif => IsGif;
        public bool ShowBrush => !IsGif;
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }
        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

        public KeyboardPropertiesModel ProposedProperties
        {
            get { return _proposedProperties; }
            set
            {
                if (Equals(value, _proposedProperties)) return;
                _proposedProperties = value;
                NotifyOfPropertyChange(() => ProposedProperties);
            }
        }

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

            ProposedProperties.GifFile = dialog.FileName;
            NotifyOfPropertyChange(() => ProposedProperties);
        }

        public override LayerPropertiesModel GetAppliedProperties()
        {
            HeightProperties.Apply(ProposedProperties);
            WidthProperties.Apply(ProposedProperties);
            OpacityProperties.Apply(ProposedProperties);

            var properties = GeneralHelpers.Clone(ProposedProperties);
            properties.Brush = Brush;
            return properties;
        }
    }
}