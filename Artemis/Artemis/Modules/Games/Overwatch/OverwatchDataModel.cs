using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchDataModel : IGameDataModel
    {
        public OverwatchStatus Status { get; set; }
    }

    public enum OverwatchStatus
    {
        Unkown,
        InMainMenu,
        InCharacterSelection,
        InGame
    }
}