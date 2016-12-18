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
            GooglePlayMusic = new GooglePlayMusic();
            Cpu = new CpuDataModel();
            Performance = new PerformanceDataModel();
            CurrentTime = new CurrentTime();
            Keyboard = new KbDataModel();
            ActiveWindow = new ActiveWindow();
        }

        public CpuDataModel Cpu { get; set; }
        public PerformanceDataModel Performance { get; set; }
        public Spotify Spotify { get; set; }
        public GooglePlayMusic GooglePlayMusic { get; set; }
        public CurrentTime CurrentTime { get; set; }
        public KbDataModel Keyboard { get; set; }
        public ActiveWindow ActiveWindow { get; set; }
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

    [MoonSharpUserData]
    public class GooglePlayMusic
    {
        public bool playing { get; set; }
        public Song song { get; set; }
        public Rating rating { get; set; }
        public Time time { get; set; }
        public string shuffle { get; set; }
        public string repeat { get; set; }
        public int volume { get; set; }
    }

    [MoonSharpUserData]
    public class Song
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string albumArt { get; set; }
    }

    [MoonSharpUserData]
    public class Rating
    {
        public bool liked { get; set; }
        public bool disliked { get; set; }
    }

    [MoonSharpUserData]
    public class Time
    {
        public int current { get; set; }
        public int total { get; set; }
    }

    [MoonSharpUserData]
    public class KbDataModel
    {
        public bool NumLock { get; set; }
        public bool CapsLock { get; set; }
        public bool ScrollLock { get; set; }
    }

    [MoonSharpUserData]
    public class ActiveWindow
    {
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
    }
}