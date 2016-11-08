using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Timers;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.UnrealTournament
{
    public class UnrealTournamentModel : GameModel
    {
        private readonly PipeServer _pipeServer;
        private readonly MetroDialogService _dialogService;
        private Timer _killTimer;
        private int _lastScore;

        public UnrealTournamentModel(DeviceManager deviceManager, PipeServer pipeServer, MetroDialogService dialogService)
            : base(deviceManager, SettingsProvider.Load<UnrealTournamentSettings>(), new UnrealTournamentDataModel())
        {
            _pipeServer = pipeServer;
            _dialogService = dialogService;
            Name = "UnrealTournament";
            ProcessName = "UE4-Win64-Shipping";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            _killTimer = new Timer(3500);
            _killTimer.Elapsed += KillTimerOnElapsed;

            FindGame();
        }

        public void FindGame()
        {
            var gameSettings = (UnrealTournamentSettings)Settings;
            // If already propertly set up, don't do anything
            if ((gameSettings.GameDirectory != null) &&
                File.Exists(gameSettings.GameDirectory + "UE4-Win64-Shipping.exe"))
                return;

            // Attempt to read the file
            if (!File.Exists(@"C:\ProgramData\Epic\UnrealEngineLauncher\LauncherInstalled.dat"))
                return;

            var json =
                JsonConvert.DeserializeObject<JObject>(
                    File.ReadAllText(@"C:\ProgramData\Epic\UnrealEngineLauncher\LauncherInstalled.dat"));
            var utEntry =
                json["InstallationList"].Children()
                    .FirstOrDefault(c => c["AppName"].Value<string>() == "UnrealTournamentDev");
            if (utEntry == null)
                return;

            var utDir = utEntry["InstallLocation"].Value<string>();
            // Use backslash in path for consistency
            utDir = utDir.Replace('/', '\\');

            if (!File.Exists(utDir + @"\UE4-Win64-Shipping.exe"))
                return;

            gameSettings.GameDirectory = utDir;
            gameSettings.Save();
            PlaceFiles();
        }

        public void PlaceFiles()
        {
            var gameSettings = (UnrealTournamentSettings)Settings;
            var path = gameSettings.GameDirectory;

            if (!File.Exists(path + @"\UE4-Win64-Shipping.exe"))
            {
                _dialogService.ShowErrorMessageBox("Please select a valid Unreal Tournament directory\n\n" +
                                                  @"By default Unreal Tournament is in C:\Program Files\Epic Games\UnrealTournament");

                gameSettings.GameDirectory = string.Empty;
                gameSettings.Save();

                Logger?.Warn("Failed to install Unreal Tournament plugin in '{0}' (path not found)", path);
                return;
            }

            // Load the ZIP from resources
            using (var stream = new MemoryStream(Resources.ut_plugin))
            {
                var archive = new ZipArchive(stream);

                try
                {
                    Directory.CreateDirectory(path + @"\UnrealTournament\Plugins\Artemis");
                    archive.ExtractToDirectory(path + @"\UnrealTournament\Plugins\Artemis", true);
                }
                catch (Exception e)
                {
                    Logger?.Error(e, "Failed to install Unreal Tournament plugin in '{0}'", path);
                    return;
                }
            }
            Logger?.Info("Installed Unreal Tournament plugin in '{0}'", path);
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;

            _killTimer.Stop();
            _pipeServer.PipeMessage -= PipeServerOnPipeMessage;
            base.Dispose();
        }

        public override void Enable()
        {
            _pipeServer.PipeMessage += PipeServerOnPipeMessage;
            _killTimer.Start();

            Initialized = true;
        }

        private void PipeServerOnPipeMessage(string message)
        {
            if (!message.Contains("\"Environment\":"))
                return;

            // Parse the JSON
            try
            {
                JsonConvert.PopulateObject(message, DataModel);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        public override void Update()
        {
            var utDataModel = (UnrealTournamentDataModel) DataModel;
            if (utDataModel.Player?.State?.Score == _lastScore)
                return;

            // Reset the timer
            _killTimer.Stop();
            _killTimer.Start();
            if (utDataModel.Player?.State != null)
            {
                // Can't go past MonsterKill in the current version of UT
                if (utDataModel.Player.KillState != KillState.MonsterKill)
                {
                    var recentKills = utDataModel.Player.State.Score - _lastScore;
                    utDataModel.Player.KillState = (KillState) ((int) utDataModel.Player.KillState + recentKills);
                }
                _lastScore = utDataModel.Player.State.Score;
            }
            else
            {
                _lastScore = 0;
            }
        }

        private void KillTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var dataModel = (UnrealTournamentDataModel) DataModel;
            if (dataModel.Player != null)
                dataModel.Player.KillState = KillState.None;
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}