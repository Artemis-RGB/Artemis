using System.Drawing;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;

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
            if (Profile == null || GameDataModel == null)
                return;

            foreach (var layerModel in Profile.Layers)
                layerModel.Update<CounterStrikeDataModel>(GameDataModel);
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
            GameDataModel = JsonConvert.DeserializeObject<CounterStrikeDataModel>(jsonString);
        }
    }
}