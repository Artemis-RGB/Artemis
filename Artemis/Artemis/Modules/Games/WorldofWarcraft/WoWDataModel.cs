using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.WorldofWarcraft
{
    public class WoWDataModel : IDataModel
    {
        public Player Player { get; set; } = new Player();
    }


    public class Player
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
    }
}