using Artemis.Modules.Abstract;

namespace Artemis.Modules.Games.WoW
{
    public class WoWSettings : ModuleSettings
    {
        public string GameDirectory { get; set; }
        public string NetworkAdapter { get; set; }
    }
}
