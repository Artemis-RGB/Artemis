using Artemis.Models.Interfaces;
using SpotifyAPI.Local.Models;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileDataModel : IDataModel
    {
        public CpuDataModel Cpu { get; set; }
        public Spotify Spotify { get; set; }

        public WindowsProfileDataModel()
        {
            Spotify = new Spotify();
            Cpu = new CpuDataModel();
        }
    }

    public class CpuDataModel
    {
        public int Core1Usage { get; set; }
        public int Core2Usage { get; set; }
        public int Core3Usage { get; set; }
        public int Core4Usage { get; set; }
        public int Core5Usage { get; set; }
        public int Core6Usage { get; set; }
        public int Core7Usage { get; set; }
        public int Core8Usage { get; set; }
    }
    public class Spotify
    {
        public bool Running { get; set; }
        public string Artist { get; set; }
        public string SongName { get; set; }
        public int SongPercentCompleted { get; set; }
        public int SpotifyVolume { get; set; }
        public string Album { get; set; }
        public bool Repeat { get; set; }
        public bool Shuffle { get; set; }
        public bool Playing { get; set; }
    }
}