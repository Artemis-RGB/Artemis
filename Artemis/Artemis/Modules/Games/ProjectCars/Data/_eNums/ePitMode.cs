using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public enum ePitMode
    {
        [Description("None")] PIT_MODE_NONE = 0,
        [Description("Pit Entry")] PIT_MODE_DRIVING_INTO_PITS,
        [Description("In Pits")] PIT_MODE_IN_PIT,
        [Description("Pit Exit")] PIT_MODE_DRIVING_OUT_OF_PITS,
        [Description("Pit Garage")] PIT_MODE_IN_GARAGE,
        //-------------
        PIT_MODE_MAX
    }
}