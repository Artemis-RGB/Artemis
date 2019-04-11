using Artemis.Core.Attributes;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Interfaces;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralDataModel : ModuleDataModel
    {
        [DataModelProperty(DisplayName = "Unique boolean")]
        public bool PropertyUniqueToThisDm { get; set; }

        public GeneralDataModel(IModule module) : base(module)
        {
        }
    }
}