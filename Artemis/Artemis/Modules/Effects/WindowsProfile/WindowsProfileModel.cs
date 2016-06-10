using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Ninject.Extensions.Logging;
using SpotifyAPI.Local;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileModel : EffectModel
    {
        private readonly ILogger _logger;
        private List<PerformanceCounter> _cores;
        private int _cpuFrames;
        private SpotifyLocalAPI _spotify;
        private bool _spotifySetupBusy;
        private bool _triedCpuFix;

        public WindowsProfileModel(ILogger logger, MainManager mainManager, WindowsProfileSettings settings)
            : base(mainManager, new WindowsProfileDataModel())
        {
            _logger = logger;
            Name = "WindowsProfile";
            Settings = settings;
        }

        public WindowsProfileSettings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            SetupCpu();
            SetupSpotify();

            Initialized = true;
        }

        public override void Update()
        {
            var dataModel = (WindowsProfileDataModel) DataModel;
            UpdateCpu(dataModel);
            UpdateSpotify(dataModel);
        }

        #region CPU

        private void SetupCpu()
        {
            try
            {
                _cores = GetPerformanceCounters();
                var coreCount = _cores.Count;
                while (coreCount < 8)
                {
                    _cores.Add(null);
                    coreCount++;
                }
            }
            catch (InvalidOperationException)
            {
                _logger.Warn("Failed to setup CPU information, try running \"lodctr /R\" as administrator.");
            }
            
        }

        private void UpdateCpu(WindowsProfileDataModel dataModel)
        {
            if (_cores == null)
                return;
            
            // CPU is only updated every 15 frames, the performance counter gives 0 if updated too often
            _cpuFrames++;
            if (_cpuFrames < 16)
                return;

            _cpuFrames = 0;

            // Update cores, not ideal but data models don't support lists. 
            if (_cores[0] != null)
                dataModel.Cpu.Core1Usage = (int) _cores[0].NextValue();
            if (_cores[1] != null)
                dataModel.Cpu.Core2Usage = (int) _cores[1].NextValue();
            if (_cores[2] != null)
                dataModel.Cpu.Core3Usage = (int) _cores[2].NextValue();
            if (_cores[3] != null)
                dataModel.Cpu.Core4Usage = (int) _cores[3].NextValue();
            if (_cores[4] != null)
                dataModel.Cpu.Core5Usage = (int) _cores[4].NextValue();
            if (_cores[5] != null)
                dataModel.Cpu.Core6Usage = (int) _cores[5].NextValue();
            if (_cores[6] != null)
                dataModel.Cpu.Core7Usage = (int) _cores[6].NextValue();
            if (_cores[7] != null)
                dataModel.Cpu.Core8Usage = (int) _cores[7].NextValue();
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<WindowsProfileDataModel>(DataModel, renderMice, renderHeadsets, false);
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

        #endregion

        #region Spotify

        public void SetupSpotify()
        {
            if (_spotifySetupBusy)
                return;

            _spotifySetupBusy = true;
            _spotify = new SpotifyLocalAPI {ListenForEvents = true};
            _spotify.OnPlayStateChange += UpdateSpotifyPlayState;
            _spotify.OnTrackChange += UpdateSpotifyTrack;
            _spotify.OnTrackTimeChange += UpdateSpotifyTrackTime;

            // Connecting can sometimes use a little bit more conviction
            Task.Factory.StartNew(() =>
            {
                var tryCount = 0;
                while (tryCount <= 10)
                {
                    tryCount++;
                    var connected = _spotify.Connect();
                    if (connected)
                        break;
                    Thread.Sleep(1000);
                }
                _spotifySetupBusy = false;
            });
        }

        public void UpdateSpotify(WindowsProfileDataModel dataModel)
        {
            if (!dataModel.Spotify.Running && SpotifyLocalAPI.IsSpotifyRunning())
                SetupSpotify();

            dataModel.Spotify.Running = SpotifyLocalAPI.IsSpotifyRunning();
        }

        private void UpdateSpotifyPlayState(object sender, PlayStateEventArgs e)
        {
            ((WindowsProfileDataModel) DataModel).Spotify.Playing = e.Playing;
        }

        private void UpdateSpotifyTrack(object sender, TrackChangeEventArgs e)
        {
            var dataModel = (WindowsProfileDataModel) DataModel;
            dataModel.Spotify.Artist = e.NewTrack.ArtistResource?.Name;
            dataModel.Spotify.SongName = e.NewTrack.TrackResource?.Name;
            dataModel.Spotify.Album = e.NewTrack.AlbumResource?.Name;
            dataModel.Spotify.SongLength = e.NewTrack.Length;
        }

        private void UpdateSpotifyTrackTime(object sender, TrackTimeChangeEventArgs e)
        {
            var dataModel = (WindowsProfileDataModel) DataModel;
            if (dataModel.Spotify.SongLength > 0)
                dataModel.Spotify.SongPercentCompleted = (int) (e.TrackTime/dataModel.Spotify.SongLength*100.0);
        }

        #endregion
    }
}