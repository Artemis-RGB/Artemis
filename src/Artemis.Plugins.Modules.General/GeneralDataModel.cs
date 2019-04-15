using Artemis.Core.Attributes;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Interfaces;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralDataModel : ModuleDataModel
    {
        public GeneralDataModel(Module module) : base(module)
        {
        }

        [DataModelProperty(DisplayName = "Unique boolean")]
        public bool PropertyUniqueToThisDm { get; set; }
    }
}