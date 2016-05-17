using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public class KeyboardPropertiesViewModel : LayerPropertiesViewModel
    {
        private bool _isGif;
        private KeyboardPropertiesModel _proposedProperties;

        public KeyboardPropertiesViewModel(IGameDataModel gameDataModel, LayerPropertiesModel properties)
            : base(gameDataModel)
        {
            var keyboardProperties = (KeyboardPropertiesModel) properties;
            ProposedProperties = GeneralHelpers.Clone(keyboardProperties);

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap(gameDataModel));

            HeightProperties = new LayerDynamicPropertiesViewModel("Height", DataModelProps, keyboardProperties);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", DataModelProps, keyboardProperties);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", DataModelProps, keyboardProperties);
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

        public bool ShowGif => IsGif;
        public bool ShowBrush => !IsGif;

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

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

        public override LayerPropertiesModel GetAppliedProperties()
        {
            
            HeightProperties.Apply(ProposedProperties);
            WidthProperties.Apply(ProposedProperties);
            OpacityProperties.Apply(ProposedProperties);

            return GeneralHelpers.Clone(ProposedProperties);
        }
    }
}