using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.GameState;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.Dota2
{
    class Dota2Model : GameModel
    {
        public Dota2Model(MainManager mainManager, Dota2Settings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "Dota2";
            ProcessName = "dota2";
            Enabled = Settings.Enabled;
            Initialized = false;
            Scale = 4;
        }

        #region Variables

        public Dota2Settings Settings { get; set; }
        public JObject D2Json { get; set; }
        public int Scale { get; set; }
        #endregion


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
            throw new NotImplementedException();
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
            }
            return bitmap;
        }


        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's CS:GO JSON
            if (!jsonString.Contains("Dota 2"))
                return;

            // Parse the JSON
            D2Json = JsonConvert.DeserializeObject<JObject>(jsonString);
        }
    }
}
