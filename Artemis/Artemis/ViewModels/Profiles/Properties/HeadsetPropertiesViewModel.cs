using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;

namespace Artemis.ViewModels.Profiles.Properties
{
    public class HeadsetPropertiesViewModel : LayerPropertiesViewModel
    {
        private LayerPropertiesModel _proposedProperties;
        private Brush _brush;

        public HeadsetPropertiesViewModel(IGameDataModel gameDataModel, LayerPropertiesModel properties)
            : base(gameDataModel)
        {
            ProposedProperties = GeneralHelpers.Clone(properties);
            Brush = GeneralHelpers.CloneAlt(ProposedProperties.Brush);
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

        public LayerPropertiesModel ProposedProperties
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
            var properties = GeneralHelpers.Clone(ProposedProperties);
            properties.Brush = Brush;
            return properties;
        }
    }
}