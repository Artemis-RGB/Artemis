using System.Drawing;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;
using Brush = System.Windows.Media.Brush;

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
            var dataModel = GameDataModel as Dota2DataModel;
            if (dataModel?.map?.daytime == null)
                return;

            var timeLeft = 240 - dataModel.map.clock_time%240;
            dataModel.map.dayCyclePercentage = (int) (100.00 / 240 * timeLeft);
        }


        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<Dota2DataModel>(keyboardRect, GameDataModel, false, true);
        }

        public override Brush GenerateMouseBrush()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            return Profile.GenerateBrush<Dota2DataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            return Profile.GenerateBrush<Dota2DataModel>(GameDataModel, LayerType.Headset, false, true);
        }

        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's Dota 2 JSON
            if (!jsonString.Contains("Dota 2"))
                return;

            // Parse the JSON
            GameDataModel = JsonConvert.DeserializeObject<Dota2DataModel>(jsonString);
        }
    }
}