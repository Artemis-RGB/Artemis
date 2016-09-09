using Artemis.Models.Interfaces;
using Artemis.Modules.Games.WoW.Data;

namespace Artemis.Modules.Games.WoW
{
    public class WoWDataModel : IDataModel
    {
        public WoWUnit Player { get; set; }
        public WoWUnit Target { get; set; }
    }
}