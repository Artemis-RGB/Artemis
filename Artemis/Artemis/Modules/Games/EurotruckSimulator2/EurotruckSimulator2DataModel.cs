using Artemis.Models.Interfaces;
using Artemis.Modules.Games.EurotruckSimulator2.Data;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2DataModel : IDataModel
    {
        public IEts2Game Game { get; set; }
        public IEts2Job Job { get; set; }
        public IEts2Navigation Navigation { get; set; }
        public IEts2Trailer Trailer { get; set; }
        public IEts2Truck Truck { get; set; }
    }
}