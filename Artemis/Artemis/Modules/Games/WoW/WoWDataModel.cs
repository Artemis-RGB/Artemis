using Artemis.Modules.Abstract;
using Artemis.Modules.Games.WoW.Data;

namespace Artemis.Modules.Games.WoW
{
    public class WoWDataModel : ModuleDataModel
    {
        public WoWUnit Player { get; set; }
        public WoWUnit Target { get; set; }
    }
}