using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Effects.WindowsProfile
{
    [MoonSharpUserData]
    public class WindowsProfileDataModel : IDataModel
    {
        public WindowsProfileDataModel()
        {
            Spotify = new Spotify();
            Cpu = new CpuDataModel();
            Performance = new PerformanceDataModel();
            CurrentTime = new CurrentTime();
        }

        public CpuDataModel Cpu { get; set; }
        public PerformanceDataModel Performance { get; set; }
        public Spotify Spotify { get; set; }
        public CurrentTime CurrentTime { get; set; }
    }

    [MoonSharpUserData]
    public class CurrentTime
    {
        public int Hours24 { get; set; }
        public int Hours12 { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
    }

    [MoonSharpUserData]
    public class CpuDataModel
    {
        public int TotalUsage { get; set; }
        public int Core1Usage { get; set; }
        public int Core2Usage { get; set; }
        public int Core3Usage { get; set; }
        public int Core4Usage { get; set; }
        public int Core5Usage { get; set; }
        public int Core6Usage { get; set; }
        public int Core7Usage { get; set; }
        public int Core8Usage { get; set; }
    }

    [MoonSharpUserData]
    public class PerformanceDataModel
    {
        public int RAMUsage { get; set; }
    }

    [MoonSharpUserData]
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
        public int SongLength { get; set; }
    }
}