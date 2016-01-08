using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Models;
using Artemis.Utilities.GameSense;
using Artemis.Utilities.GameSense.JsonModels;
using Artemis.Utilities.Keyboard;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeModel : GameModel
    {
        private readonly MainModel _mainModel;

        // TODO: Make functional (CS' new gamestate intergration broke this)
        public CounterStrikeModel(MainModel mainModel)
        {
            _mainModel = mainModel;
            Name = "CounterStrike";
            ProcessName = "csgo";
            Scale = 4;

            AmmoRect = new KeyboardRectangle(Scale, 0, 0, 84, 24, new List<Color> {Color.Blue, Color.Red},
                LinearGradientMode.Horizontal);
        }

        public int Scale { get; set; }

        public CounterStrikeJson CsJson { get; set; }
        public KeyboardRectangle AmmoRect { get; set; }

        public override void Dispose()
        {
            _mainModel.GameSenseWebServer.GameDataReceived -= HandleGameData;
        }

        public override void Enable()
        {
            _mainModel.GameSenseWebServer.GameDataReceived += HandleGameData;
        }

        public override void Update()
        {
            if (CsJson?.player.weapons == null)
                return;

            var activeWeapon = CsJson.player.weapons.FirstOrDefault(w => w.Value.state.Equals("active"));

            // Update the ammo display
            var ammoPercentage = 0;
            if (activeWeapon.Value?.ammo_clip_max > 0)
                ammoPercentage =
                    (int) ((Math.Ceiling(100.00/activeWeapon.Value.ammo_clip_max))*activeWeapon.Value.ammo_clip);

            AmmoRect.Height = ammoPercentage;
            if (ammoPercentage < 30)
                AmmoRect.StartBlink(1000);
            else
                AmmoRect.StopBlink();
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(21, 6);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                AmmoRect.Draw(g);
            }
            return bitmap;
        }

        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's CS:GO JSON
            if (!jsonString.Contains("Counter-Strike: Global Offensive"))
                return;

            // Parse the JSON
            CsJson = JsonConvert.DeserializeObject<CounterStrikeJson>(jsonString);
        }
    }
}