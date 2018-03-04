using Artemis.Core.Attributes;
using Artemis.Plugins.Interfaces;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralDataModel : IModuleDataModel
    {
        [DataModelProperty(DisplayName = "Unique boolean")]
        public bool PropertyUniqueToThisDm { get; set; }
    }
}