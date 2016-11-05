using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;
using SpotifyAPI.Local;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileModel : EffectModel
    {
        private readonly ILogger _logger;
        private List<PerformanceCounter> _cores;
        private int _cpuFrames;
        private PerformanceCounter _overallCpu;
        private SpotifyLocalAPI _spotify;
        private bool _spotifySetupBusy;
        private DateTime _lastMusicUpdate;

        public WindowsProfileModel(ILogger logger, MainManager mainManager)
            : base(mainManager, SettingsProvider.Load<WindowsProfileSettings>(), new WindowsProfileDataModel())
        {
            _logger = logger;
            _lastMusicUpdate = DateTime.Now;

            Name = "WindowsProfile";
        }

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
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
            UpdateMusicPlayers(dataModel);
            UpdateDay(dataModel);
        }

        #region Current Time

        private void UpdateDay(WindowsProfileDataModel dataModel)
        {
            var now = DateTime.Now;
            dataModel.CurrentTime.Hours24 = int.Parse(now.ToString("HH"));
            dataModel.CurrentTime.Hours12 = int.Parse(now.ToString("hh"));
            dataModel.CurrentTime.Minutes = int.Parse(now.ToString("mm"));
            dataModel.CurrentTime.Seconds = int.Parse(now.ToString("ss"));
        }

        #endregion

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
                _overallCpu = GetOverallPerformanceCounter();
            }
            catch (InvalidOperationException)
            {
                _logger.Warn("Failed to setup CPU information, try running \"lodctr /R\" as administrator.");
            }
        }

        private void UpdateCpu(WindowsProfileDataModel dataModel)
        {
            if ((_cores == null) || (_overallCpu == null))
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

            //From Ted - Let's get overall RAM and CPU usage here           
            dataModel.Cpu.TotalUsage = (int) _overallCpu.NextValue();

            var phav = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            var tot = PerformanceInfo.GetTotalMemoryInMiB();
            var percentFree = phav/(decimal) tot*100;
            var percentOccupied = 100 - percentFree;

            dataModel.Performance.RAMUsage = (int) percentOccupied;
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly, false);
        }

        public static PerformanceCounter GetOverallPerformanceCounter()
        {
            var cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };

            return cpuCounter;
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
            _spotify = new SpotifyLocalAPI();

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

        public void UpdateMusicPlayers(WindowsProfileDataModel dataModel)
        {
            // This is quite resource hungry so only update it once every two seconds
            if (DateTime.Now - _lastMusicUpdate < TimeSpan.FromSeconds(2))
                return;
            _lastMusicUpdate = DateTime.Now;

            UpdateSpotify(dataModel);
            UpdateGooglePlayMusic(dataModel);
        }

        private void UpdateSpotify(WindowsProfileDataModel dataModel)
        {
            // Spotify
            if (!dataModel.Spotify.Running && SpotifyLocalAPI.IsSpotifyRunning())
                SetupSpotify();

            var status = _spotify.GetStatus();
            if (status == null)
                return;

            dataModel.Spotify.Playing = status.Playing;
            dataModel.Spotify.Running = SpotifyLocalAPI.IsSpotifyRunning();

            if (status.Track != null)
            {
                dataModel.Spotify.Artist = status.Track.ArtistResource?.Name;
                dataModel.Spotify.SongName = status.Track.TrackResource?.Name;
                dataModel.Spotify.Album = status.Track.AlbumResource?.Name;
                dataModel.Spotify.SongLength = status.Track.Length;
            }

            if (dataModel.Spotify.SongLength > 0)
            {
                dataModel.Spotify.SongPercentCompleted =
                    (int) (status.PlayingPosition/dataModel.Spotify.SongLength*100.0);
            }
        }

        private void UpdateGooglePlayMusic(WindowsProfileDataModel dataModel)
        {
            // Google Play Music
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var json = appData + @"\Google Play Music Desktop Player\json_store\playback.json";
            if (!File.Exists(json))
                return;

            dataModel.GooglePlayMusic = JsonConvert.DeserializeObject<GooglePlayMusic>(File.ReadAllText(json));
        }

        #endregion
    }
}