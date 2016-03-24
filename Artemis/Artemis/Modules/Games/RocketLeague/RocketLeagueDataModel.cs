using System.Collections.Generic;
using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Modules.Games.RocketLeague
{
    internal class RocketLeagueDataModel : IGameDataModel
    {
        public int Boost { get; set; }
        public List<GeneralHelpers.PropertyCollection> Properties { get; }
    }
}