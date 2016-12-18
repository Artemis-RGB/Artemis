using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    //public class eGameState {public eGameState eGameState { get; set; }}

    public enum eGameState
    {
        [Description("Waiting for game to start...")] GAME_EXITED = 0,
        [Description("In Menus")] GAME_FRONT_END,
        [Description("In Session")] GAME_INGAME_PLAYING,
        [Description("Game Paused")] GAME_INGAME_PAUSED,
        //-------------
        GAME_MAX
    }
}