using System;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeModel : ModuleModel
    {
        private readonly MetroDialogService _dialogService;
        private readonly GameStateWebServer _gameStateWebServer;
        private DateTime _lastHeadshot;
        private int _lastHeadshots;
        private DateTime _lastKill;
        private int _lastKills;

        public CounterStrikeModel(DeviceManager deviceManager, LuaManager luaManager, MetroDialogService dialogService,
            GameStateWebServer gameStateWebServer) : base(deviceManager, luaManager)
        {
            _gameStateWebServer = gameStateWebServer;
            _dialogService = dialogService;

            Settings = SettingsProvider.Load<CounterStrikeSettings>();
            DataModel = new CounterStrikeDataModel();
            ProcessNames.Add("csgo");

            FindGameDir();
            PlaceConfigFile();
        }

        public override string Name => "CounterStrike";
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
            if (DataModel == null)
                return;

            var dm = (CounterStrikeDataModel) DataModel;
            if (dm.player != null)
            {
                // Detect active weapon
                if (dm.player.weapons.weapon_0?.state == "active")
                    dm.player.weapons.active_weapon = dm.player.weapons.weapon_0;
                else if (dm.player.weapons.weapon_1?.state == "active")
                    dm.player.weapons.active_weapon = dm.player.weapons.weapon_1;
                else if (dm.player.weapons.weapon_2?.state == "active")
                    dm.player.weapons.active_weapon = dm.player.weapons.weapon_2;

                // Detect a kill
                if (dm.player.state.round_kills > _lastKills)
                {
                    dm.player.state.made_kill = true;
                    _lastKill = DateTime.Now;
                }
                else if (dm.player.state.made_kill && DateTime.Now - _lastKill > TimeSpan.FromMilliseconds(500))
                {
                    dm.player.state.made_kill = false;
                }
                _lastKills = dm.player.state.round_kills;

                // Detect a headshot
                if (dm.player.state.round_killhs > _lastHeadshots)
                {
                    dm.player.state.made_headshot = true;
                    _lastHeadshot = DateTime.Now;
                }
                else if (dm.player.state.made_headshot && DateTime.Now - _lastHeadshot > TimeSpan.FromMilliseconds(500))
                {
                    dm.player.state.made_headshot = false;
                }
                _lastHeadshots = dm.player.state.round_killhs;

                // Detect a round win

                // Detect a round loss
            }

            DataModel = dm;
        }

        public void FindGameDir()
        {
            var gameSettings = (CounterStrikeSettings) Settings;
            // If already propertly set up, don't do anything
            if (gameSettings.GameDirectory != null && File.Exists(gameSettings.GameDirectory + "csgo.exe") &&
                File.Exists(gameSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg"))
                return;

            var dir = GeneralHelpers.FindSteamGame(@"\Counter-Strike Global Offensive\csgo.exe");
            gameSettings.GameDirectory = dir ?? string.Empty;
            gameSettings.Save();
        }

        public void PlaceConfigFile()
        {
            var gameSettings = (CounterStrikeSettings) Settings;
            if (gameSettings.GameDirectory == string.Empty)
                return;

            var path = gameSettings.GameDirectory;
            if (Directory.Exists(path + "/csgo/cfg"))
            {
                var cfgFile = Resources.csgoGamestateConfiguration.Replace("{{port}}",
                    _gameStateWebServer.Port.ToString());
                File.WriteAllText(path + "/csgo/cfg/gamestate_integration_artemis.cfg", cfgFile);

                return;
            }

            _dialogService.ShowErrorMessageBox("Please select a valid CS:GO directory\n\n" +
                                               @"By default CS:GO is in \SteamApps\common\Counter-Strike Global Offensive");

            gameSettings.GameDirectory = string.Empty;
            gameSettings.Save();
        }

        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's CS:GO JSON
            if (!jsonString.Contains("Counter-Strike: Global Offensive"))
                return;

            // Parse the JSON
            try
            {
                if (DataModel == null)
                    DataModel = new CounterStrikeDataModel();
                JsonConvert.PopulateObject(jsonString, DataModel);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to deserialize CS:GO JSON");
                throw;
            }
        }
    }
}