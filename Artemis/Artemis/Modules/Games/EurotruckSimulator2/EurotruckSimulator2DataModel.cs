using Artemis.Models.Interfaces;
using Ets2SdkClient;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2DataModel : IDataModel
    {
        public Ets2Telemetry._Axilliary Axilliary { get; set; }
        public Ets2Telemetry._Controls Controls { get; set; }
        public Ets2Telemetry._Damage Damage { get; set; }
        public Ets2Telemetry._Drivetrain Drivetrain { get; set; }
        public Ets2Telemetry._Job Job { get; set; }
        public Ets2Telemetry._Lights Lights { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerId { get; set; }
        public Ets2Telemetry._Physics Physics { get; set; }
    }
}