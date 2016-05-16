using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;

namespace Artemis.ViewModels.LayerEditor
{
    public class MousePropertiesViewModel : LayerPropertiesViewModel
    {
        public MousePropertiesViewModel(IGameDataModel gameDataModel) : base(gameDataModel)
        {
        }

        public override LayerPropertiesModel GetAppliedProperties()
        {
            return null;
        }
    }
}