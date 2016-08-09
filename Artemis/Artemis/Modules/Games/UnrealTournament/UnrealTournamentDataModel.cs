using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.UnrealTournament
{
    public class UnrealTournamentDataModel : IDataModel
    {
        public State State { get; set; }
        public Environment Environment { get; set; }
        public Player Player { get; set; }
    }

    public enum State
    {
        MainMenu,
        Spectating,
        Alive,
        Dead
    }

    public class Player
    {
        public string Name { get; set; }
        public string Team { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public string Powerup { get; set; }
    }

    public class Environment
    {
        public string Mode { get; set; }
        public string MapName { get; set; }
    }
}