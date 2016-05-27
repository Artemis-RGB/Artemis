using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;

namespace Artemis.ViewModels.Profiles.Properties
{
    public class FolderPropertiesViewModel : LayerPropertiesViewModel
    {
        private LayerPropertiesModel _proposedProperties;

        public FolderPropertiesViewModel(IGameDataModel gameDataModel, LayerPropertiesModel properties)
            : base(gameDataModel)
        {
            ProposedProperties = GeneralHelpers.Clone(properties);
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
            return GeneralHelpers.Clone(ProposedProperties);
        }
    }
}