using System.Collections.Generic;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2Model : GameModel
    {
        private readonly MetroDialogService _dialogService;
        private readonly GameStateWebServer _gameStateWebServer;

        public Dota2Model(DeviceManager deviceManager, LuaManager luaManager, GameStateWebServer gameStateWebServer,
            MetroDialogService dialogService)
            : base(deviceManager, luaManager, SettingsProvider.Load<Dota2Settings>(), new Dota2DataModel())
        {
            _gameStateWebServer = gameStateWebServer;
            _dialogService = dialogService;

            Name = "Dota2";
            ProcessName = "dota2";
            Enabled = Settings.Enabled;
            Initialized = false;
            Scale = 4;

            FindGameDir();
            PlaceConfigFile();
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _gameStateWebServer.GameDataReceived -= HandleGameData;
            base.Dispose();
        }

        public override void Enable()
        {
            base.Enable();

            _gameStateWebServer.GameDataReceived += HandleGameData;
            Initialized = true;
        }

        public override void Update()
        {
            UpdateDay();
        }

        public void FindGameDir()
        {
            var gameSettings = (Dota2Settings) Settings;
            // If already propertly set up, don't do anything
            if ((gameSettings.GameDirectory != null) && File.Exists(gameSettings.GameDirectory + "csgo.exe") &&
                File.Exists(gameSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg"))
                return;

            var dir = GeneralHelpers.FindSteamGame(@"\dota 2 beta\game\bin\win32\dota2.exe");
            // Remove subdirectories where they stuck the executable
            dir = dir?.Substring(0, dir.Length - 15);

            gameSettings.GameDirectory = dir ?? string.Empty;
            gameSettings.Save();
        }

        public void PlaceConfigFile()
        {
            var gameSettings = (Dota2Settings) Settings;
            if (gameSettings.GameDirectory == string.Empty)
                return;
            if (Directory.Exists(gameSettings.GameDirectory + "/game/dota/cfg"))
            {
                var cfgFile = Resources.dotaGamestateConfiguration.Replace("{{port}}",
                    _gameStateWebServer.Port.ToString());
                try
                {
                    File.WriteAllText(
                        gameSettings.GameDirectory +
                        "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg", cfgFile);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(gameSettings.GameDirectory + "/game/dota/cfg/gamestate_integration/");
                    File.WriteAllText(
                        gameSettings.GameDirectory +
                        "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg",
                        cfgFile);
                }

                return;
            }

            _dialogService.ShowErrorMessageBox("Please select a valid Dota 2 directory\n\n" +
                                               @"By default Dota 2 is in \SteamApps\common\dota 2 beta");
            gameSettings.GameDirectory = string.Empty;
            gameSettings.Save();
        }

        private void UpdateDay()
        {
            var dataModel = DataModel as Dota2DataModel;
            if (dataModel?.map?.daytime == null)
                return;

            var timeLeft = 240 - dataModel.map.clock_time % 240;
            dataModel.map.dayCyclePercentage = (int) (100.00 / 240 * timeLeft);
        }

        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's Dota 2 JSON
            if (!jsonString.Contains("Dota 2"))
                return;

            // Parse the JSON
            DataModel = JsonConvert.DeserializeObject<Dota2DataModel>(jsonString);
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}