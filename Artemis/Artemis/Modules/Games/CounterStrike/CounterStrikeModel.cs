using System.Drawing;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeModel : GameModel
    {
        public CounterStrikeModel(MainManager mainManager, CounterStrikeSettings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "CounterStrike";
            ProcessName = "csgo";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public CounterStrikeSettings Settings { get; set; }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            MainManager.GameStateWebServer.GameDataReceived -= HandleGameData;
        }

        public override void Enable()
        {
            Initialized = false;

            GameDataModel = new CounterStrikeDataModel();
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
            Bitmap bitmap = null;
            Profile.DrawingVisual.Dispatcher.Invoke(delegate
            {
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    // Setup the DrawingVisual's size
                    drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                        null, keyboardRect);

                    // Draw the layers
                    foreach (var layerModel in Profile.Layers)
                        layerModel.Draw<CounterStrikeDataModel>(GameDataModel, drawingContext);

                    // Remove the clip
                    drawingContext.Pop();
                }

                bitmap = ImageUtilities.DrawinVisualToBitmap(visual, keyboardRect);
            });
            return bitmap;
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