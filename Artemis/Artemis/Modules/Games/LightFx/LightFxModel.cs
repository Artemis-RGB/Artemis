using System;
using System.Collections.Generic;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.GameState;

namespace Artemis.Modules.Games.LightFx
{
    public class LightFxModel : GameModel
    {
        public LightFxModel(DeviceManager deviceManager, GameStateWebServer gameStateWebServer)
            : base(deviceManager, SettingsProvider.Load<LightFxSettings>(), new LightFxDataModel())
        {
            Name = "LightFX";
            ProcessName = "LoL";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            // This model can enable itself by changing its process name to the currently running Light FX game.
            gameStateWebServer.GameDataReceived += GameStateWebServerOnGameDataReceived;
        }

        private void GameStateWebServerOnGameDataReceived(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();
            // Ensure it's Light FX JSON
            if (!jsonString.Contains("lightFxState"))
                return;

            // Deserialize and data

            // Setup process name

        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}