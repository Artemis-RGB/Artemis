using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public enum eSessionState
    {
        [Description("No Session")] SESSION_INVALID = 0,
        [Description("Practise")] SESSION_PRACTICE,
        [Description("Testing")] SESSION_TEST,
        [Description("Qualifying")] SESSION_QUALIFY,
        [Description("Formation Lap")] SESSION_FORMATIONLAP,
        [Description("Racing")] SESSION_RACE,
        [Description("Time Trial")] SESSION_TIME_ATTACK,
        //-------------
        SESSION_MAX
    }
}