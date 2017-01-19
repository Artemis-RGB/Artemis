using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
using CSCore.CoreAudioAPI;
using Newtonsoft.Json;
using SpotifyAPI.Local;

namespace Artemis.Modules.General.GeneralProfile
{
    public class GeneralProfileModel : ModuleModel
    {
        private DateTime _lastMusicUpdate;
        private SpotifyLocalAPI _spotify;
        private bool _spotifySetupBusy;

        public GeneralProfileModel(DeviceManager deviceManager, LuaManager luaManager,
            AudioCaptureManager audioCaptureManager) : base(deviceManager, luaManager)
        {
            _lastMusicUpdate = DateTime.Now;

            Settings = SettingsProvider.Load<GeneralProfileSettings>();
            DataModel = new GeneralProfileDataModel();

            audioCaptureManager.AudioDeviceChanged += AudioDeviceChanged;
        }

        public override string Name => "GeneralProfile";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => false;

        public override void Enable()
        {
            SetupCpu();
            SetupSpotify();
            SetupAudio();

            base.Enable();
        }

        public override void Update()
        {
            var dataModel = (GeneralProfileDataModel) DataModel;
            UpdateCpu(dataModel);
            UpdateMusicPlayers(dataModel);
            UpdateDay(dataModel);
            UpdateKeyStates(dataModel);
            UpdateActiveWindow(dataModel);
            UpdateAudio(dataModel);
        }

        #region Current Time

        private void UpdateDay(GeneralProfileDataModel dataModel)
        {
            var now = DateTime.Now;
            dataModel.CurrentTime.Hours24 = int.Parse(now.ToString("HH"));
            dataModel.CurrentTime.Hours12 = int.Parse(now.ToString("hh"));
            dataModel.CurrentTime.Minutes = int.Parse(now.ToString("mm"));
            dataModel.CurrentTime.Seconds = int.Parse(now.ToString("ss"));
        }

        #endregion

        #region Audio

        private MMDevice _defaultRecording;
        private MMDevice _defaultPlayback;
        private AudioMeterInformation _recordingInfo;
        private AudioMeterInformation _playbackInfo;

        private void SetupAudio()
        {
            _defaultRecording = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            _recordingInfo = AudioMeterInformation.FromDevice(_defaultRecording);
            _defaultPlayback = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _playbackInfo = AudioMeterInformation.FromDevice(_defaultPlayback);
        }

        private void AudioDeviceChanged(object sender, AudioDeviceChangedEventArgs e)
        {
            _defaultRecording = e.DefaultRecording;
            _recordingInfo = AudioMeterInformation.FromDevice(_defaultRecording);
            _defaultPlayback = e.DefaultPlayback;
            _playbackInfo = AudioMeterInformation.FromDevice(_defaultPlayback);
        }

        private void UpdateAudio(GeneralProfileDataModel dataModel)
        {
            // Update microphone, only bother with OverallPeak
            if (_defaultRecording != null)
            {
    
                dataModel.Audio.Recording.OverallPeak = _recordingInfo.PeakValue;
            }

            if (_defaultPlayback == null)
                return;

            // Update volume if a default device is found
            dataModel.Audio.Volume = AudioEndpointVolume.FromDevice(_defaultPlayback).GetMasterVolumeLevelScalar();

            // Update speakers, only do overall, left and right for now
            // TODO: When adding list support lets do all channels
            var peakValues = _playbackInfo.GetChannelsPeakValues();
            dataModel.Audio.Playback.OverallPeak = _playbackInfo.PeakValue;
            dataModel.Audio.Playback.LeftPeak = peakValues[0];
            dataModel.Audio.Playback.LeftPeak = peakValues[1];
        }

        #endregion

        #region CPU

        private List<PerformanceCounter> _cores;
        private int _cpuFrames;
        private PerformanceCounter _overallCpu;

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
                Logger?.Warn("Failed to setup CPU information, try running \"lodctr /R\" as administrator.");
            }
        }

        private void UpdateCpu(GeneralProfileDataModel dataModel)
        {
            if (_cores == null || _overallCpu == null)
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
            var percentFree = phav / (decimal) tot * 100;
            var percentOccupied = 100 - percentFree;

            dataModel.Performance.RAMUsage = (int) percentOccupied;
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

        #region Music

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
                    try
                    {
                        tryCount++;
                        var connected = _spotify.Connect();
                        if (connected)
                            break;
                        Thread.Sleep(1000);
                    }
                    catch (WebException)
                    {
                        break;
                    }
                _spotifySetupBusy = false;
            });
        }

        public void UpdateMusicPlayers(GeneralProfileDataModel dataModel)
        {
            // This is quite resource hungry so only update it once every two seconds
            if (DateTime.Now - _lastMusicUpdate < TimeSpan.FromSeconds(2))
                return;
            _lastMusicUpdate = DateTime.Now;

            UpdateSpotify(dataModel);
            UpdateGooglePlayMusic(dataModel);
        }

        private void UpdateSpotify(GeneralProfileDataModel dataModel)
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
                dataModel.Spotify.SongPercentCompleted =
                    (int) (status.PlayingPosition / dataModel.Spotify.SongLength * 100.0);
        }

        private void UpdateGooglePlayMusic(GeneralProfileDataModel dataModel)
        {
            // Google Play Music
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var json = appData + @"\Google Play Music Desktop Player\json_store\playback.json";
            if (!File.Exists(json))
                return;

            dataModel.GooglePlayMusic = JsonConvert.DeserializeObject<GooglePlayMusic>(File.ReadAllText(json));
        }

        #endregion

        #region System

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        private void UpdateKeyStates(GeneralProfileDataModel dataModel)
        {
            dataModel.Keyboard.NumLock = ((ushort) GetKeyState(0x90) & 0xffff) != 0;
            dataModel.Keyboard.CapsLock = ((ushort) GetKeyState(0x14) & 0xffff) != 0;
            dataModel.Keyboard.ScrollLock = ((ushort) GetKeyState(0x91) & 0xffff) != 0;
        }

        private void UpdateActiveWindow(GeneralProfileDataModel dataModel)
        {
            dataModel.ActiveWindow.ProcessName = ActiveWindowHelper.ActiveWindowProcessName;
            dataModel.ActiveWindow.WindowTitle = ActiveWindowHelper.ActiveWindowWindowTitle;
        }

        #endregion
    }
}