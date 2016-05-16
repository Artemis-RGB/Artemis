using System;
using System.Drawing;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeModel : GameModel
    {
        public CounterStrikeModel(MainManager mainManager, CounterStrikeSettings settings) : base(mainManager, settings, new CounterStrikeDataModel())
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
        }

        public override void Enable()
        {
            Initialized = false;

            MainManager.GameStateWebServer.GameDataReceived += HandleGameData;

            Initialized = true;
        }

        public override void Update()
        {
            // TODO: Set up active weapon in the datamodel
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.KeyboardManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<CounterStrikeDataModel>(keyboardRect, GameDataModel);
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
                GameDataModel = JsonConvert.DeserializeObject<CounterStrikeDataModel>(jsonString);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to deserialize CS:GO JSON");
                throw;
            }
            
        }
    }
}