using Artemis.Core.Attributes;
using Artemis.Core.Plugins.Interfaces;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralDataModel : IModuleDataModel
    {
        [DataModelProperty(DisplayName = "Unique boolean")]
        public bool PropertyUniqueToThisDm { get; set; }
    }
}