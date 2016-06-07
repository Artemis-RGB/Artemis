using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileModel : EffectModel
    {
        private List<PerformanceCounter> _cores;
        private int _cpuFrames;
        private readonly SpotifyLocalAPI _spotify;

        public WindowsProfileModel(MainManager mainManager, WindowsProfileSettings settings)
            : base(mainManager, new WindowsProfileDataModel())
        {
            Name = "WindowsProfile";
            Settings = settings;
            _spotify = new SpotifyLocalAPI();
        }

        public WindowsProfileSettings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            // Setup CPU cores
            _cores = GetPerformanceCounters();
            var coreCount = _cores.Count;
            while (coreCount < 8)
            {
                _cores.Add(null);
                coreCount++;
            }

            if (SpotifyLocalAPI.IsSpotifyRunning())
            {
                _spotify.Connect();
            }

            Initialized = true;
        }

        public override void Update()
        {
            var dataModel = (WindowsProfileDataModel) DataModel;
            UpdateCpu(dataModel);
            UpdateSpotify(dataModel);
        }

        private void UpdateCpu(WindowsProfileDataModel dataModel)
        {
            // CPU is only updated every 15 frames, the performance counter gives 0 if updated too often
            _cpuFrames++;
            if (_cpuFrames < 16)
                return;

            _cpuFrames = 0;

            // Update cores, not ideal but data models don't support lists. 
            if (_cores[0] != null)
                dataModel.Cpu.Core1Usage = (int)_cores[0].NextValue();
            if (_cores[1] != null)
                dataModel.Cpu.Core2Usage = (int)_cores[1].NextValue();
            if (_cores[2] != null)
                dataModel.Cpu.Core3Usage = (int)_cores[2].NextValue();
            if (_cores[3] != null)
                dataModel.Cpu.Core4Usage = (int)_cores[3].NextValue();
            if (_cores[4] != null)
                dataModel.Cpu.Core5Usage = (int)_cores[4].NextValue();
            if (_cores[5] != null)
                dataModel.Cpu.Core6Usage = (int)_cores[5].NextValue();
            if (_cores[6] != null)
                dataModel.Cpu.Core7Usage = (int)_cores[6].NextValue();
            if (_cores[7] != null)
                dataModel.Cpu.Core8Usage = (int)_cores[7].NextValue();
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<WindowsProfileDataModel>(DataModel, renderMice, renderHeadsets, true);
        }

        public static List<PerformanceCounter> GetPerformanceCounters()
        {
            var performanceCounters = new List<PerformanceCounter>();
            var procCount = Environment.ProcessorCount;
            for (var i = 0; i < procCount; i++)
            {
                var pc = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                performanceCounters.Add(pc);
            }
            return performanceCounters;
        }

        public void UpdateSpotify(WindowsProfileDataModel dataModel)
        {
            StatusResponse status = _spotify.GetStatus();
            if (status == null)
                return;

            dataModel.Spotify.Artist = status.Track.ArtistResource.Name;
            dataModel.Spotify.SongName = status.Track.TrackResource.Name;
            dataModel.Spotify.SongPercentCompleted = (int) (status.PlayingPosition/status.Track.Length*100.0);
            dataModel.Spotify.SpotifyVolume = (int)(status.Volume * 100);
            dataModel.Spotify.Album = status.Track.AlbumResource.Name;
            dataModel.Spotify.Repeat = status.Repeat;
            dataModel.Spotify.Shuffle = status.Shuffle;
            dataModel.Spotify.Playing = status.Playing;
        }
    }
}