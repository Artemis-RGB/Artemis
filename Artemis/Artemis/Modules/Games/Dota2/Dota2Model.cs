using System.Collections.Generic;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.Dota2
{
    internal class Dota2Model : GameModel
    {
        public Dota2Model(MainManager mainManager, Dota2Settings settings)
            : base(mainManager, settings, new Dota2DataModel())
        {
            Name = "Dota2";
            ProcessName = "dota2";
            Settings = settings;
            Enabled = Settings.Enabled;
            Initialized = false;
            Scale = 4;
        }

        public int Scale { get; set; }
        public new Dota2Settings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            MainManager.GameStateWebServer.GameDataReceived -= HandleGameData;
        }

        public override void Enable()
        {
            Initialized = false;

            MainManager.GameStateWebServer.GameDataReceived += HandleGameData;
            Initialized = true;
        }

        public override void Update()
        {
            UpdateDay();
        }

        private void UpdateDay()
        {
            var dataModel = DataModel as Dota2DataModel;
            if (dataModel?.map?.daytime == null)
                return;

            var timeLeft = 240 - dataModel.map.clock_time%240;
            dataModel.map.dayCyclePercentage = (int) (100.00/240*timeLeft);
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