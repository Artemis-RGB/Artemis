using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2Model : ModuleModel
    {
        private readonly MetroDialogService _dialogService;
        private readonly GameStateWebServer _gameStateWebServer;


        public Dota2Model(DeviceManager deviceManager, LuaManager luaManager, MetroDialogService dialogService,
            GameStateWebServer gameStateWebServer) : base(deviceManager, luaManager)
        {
            _gameStateWebServer = gameStateWebServer;
            _dialogService = dialogService;

            Settings = SettingsProvider.Load<Dota2Settings>();
            DataModel = new Dota2DataModel();
            ProcessNames.Add("dota2");

            FindGameDir();
            PlaceConfigFile();
        }

        public override string Name => "Dota2";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Dispose()
        {
            base.Dispose();
            _gameStateWebServer.GameDataReceived -= HandleGameData;
        }

        public override void Enable()
        {
            _gameStateWebServer.GameDataReceived += HandleGameData;
            base.Enable();
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
                    File.WriteAllText(gameSettings.GameDirectory +
                                      "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg", cfgFile);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(gameSettings.GameDirectory + "/game/dota/cfg/gamestate_integration/");
                    File.WriteAllText(gameSettings.GameDirectory +
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
    }
}