using System.Windows.Forms;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Properties
{
    public class KeyboardPropertiesViewModel : LayerPropertiesViewModel
    {
        private Brush _brush;
        private bool _isGif;
        private KeyboardPropertiesModel _proposedProperties;

        public KeyboardPropertiesViewModel(IGameDataModel gameDataModel, LayerPropertiesModel properties)
            : base(gameDataModel)
        {
            var keyboardProperties = (KeyboardPropertiesModel) properties;
            ProposedProperties = GeneralHelpers.Clone(keyboardProperties);
            Brush = ProposedProperties.Brush.CloneCurrentValue();

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap(gameDataModel));

            HeightProperties = new LayerDynamicPropertiesViewModel("Height", DataModelProps, keyboardProperties);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", DataModelProps, keyboardProperties);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", DataModelProps, keyboardProperties);
        }


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

        public bool ShowGif => IsGif;

        public bool ShowBrush => !IsGif;

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }

        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }

        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

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