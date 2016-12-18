using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public enum eFlagColors
    {
        [Description("No Flag")] FLAG_COLOUR_NONE = 0, // Not used for actual flags, only for some query functions
        [Description("Green Flag")] FLAG_COLOUR_GREEN, // End of danger zone, or race started
        [Description("Blue Flag")] FLAG_COLOUR_BLUE, // Faster car wants to overtake the participant
        [Description("White Flag")] FLAG_COLOUR_WHITE, // Approaching a slow car
        [Description("Yellow Flag")] FLAG_COLOUR_YELLOW, // Danger on the racing surface itself
        [Description("Double Yellow Flag")] FLAG_COLOUR_DOUBLE_YELLOW,
        // Danger that wholly or partly blocks the racing surface
        [Description("Black Flag")] FLAG_COLOUR_BLACK, // Participant disqualified
        [Description("Chequered Flag")] FLAG_COLOUR_CHEQUERED, // Chequered flag
        //-------------
        FLAG_COLOUR_MAX
    }
}