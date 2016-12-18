using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public enum eFlagReason
    {
        [Description("No Reason")] FLAG_REASON_NONE = 0,
        [Description("Solo Crash")] FLAG_REASON_SOLO_CRASH,
        [Description("Vehicle Crash")] FLAG_REASON_VEHICLE_CRASH,
        [Description("Vehicle Obstruction")] FLAG_REASON_VEHICLE_OBSTRUCTION,
        //-------------
        FLAG_REASON_MAX
    }
}