using System;
using System.Collections.Generic;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeModel : GameModel
    {
        private DateTime _lastHeadshot;
        private int _lastHeadshots;
        private DateTime _lastKill;
        private int _lastKills;

        public CounterStrikeModel(MainManager mainManager)
            : base(mainManager, SettingsProvider.Load<CounterStrikeSettings>(), new CounterStrikeDataModel())
        {
            Name = "CounterStrike";
            ProcessName = "csgo";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public ILogger Logger { get; set; }
        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            MainManager.GameStateWebServer.GameDataReceived -= HandleGameData;
            base.Dispose();
        }

        public override void Enable()
        {
            Initialized = false;

            MainManager.GameStateWebServer.GameDataReceived += HandleGameData;

            Initialized = true;
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
                else if (dm.player.state.made_kill && (DateTime.Now - _lastKill > TimeSpan.FromMilliseconds(500)))
                    dm.player.state.made_kill = false;
                _lastKills = dm.player.state.round_kills;

                // Detect a headshot
                if (dm.player.state.round_killhs > _lastHeadshots)
                {
                    dm.player.state.made_headshot = true;
                    _lastHeadshot = DateTime.Now;
                }
                else if (dm.player.state.made_headshot && (DateTime.Now - _lastHeadshot > TimeSpan.FromMilliseconds(500)))
                    dm.player.state.made_headshot = false;
                _lastHeadshots = dm.player.state.round_killhs;

                // Detect a round win

                // Detect a round loss
            }

            DataModel = dm;
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

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}