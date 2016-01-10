using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Models;
using Artemis.Utilities.GameSense;
using Artemis.Utilities.Keyboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            AmmoRect = new KeyboardRectangle(Scale, 0, 0, 16*Scale, 1*Scale, new List<Color> {Color.Blue, Color.Red},
                LinearGradientMode.Horizontal);
            TeamRect = new KeyboardRectangle(Scale, 0, 1*Scale, 21*Scale, 8*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal);
            EventRect = new KeyboardRectangle(Scale, 0, 1*Scale, 21*Scale, 8*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal);
        }

        public KeyboardRectangle EventRect { get; set; }
        public KeyboardRectangle TeamRect { get; set; }
        public KeyboardRectangle AmmoRect { get; set; }
        public JObject CsJson { get; set; }

        public bool DrawingSmoke { get; set; }
        public bool DrawingFlash { get; set; }

        public int Scale { get; set; }

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
            if (CsJson == null)
                return;

            UpdateAmmo();
            UpdateTeam();
            UpdateHealth();
            UpdateFlash();
            UpdateSmoke();
        }

        private void UpdateHealth()
        {
            if (CsJson["player"]?["state"]?["health"] == null)
                return;

            var health = CsJson["player"]["state"]["health"].Value<int>();
            if (health > 25)
                return;

            TeamRect.Colors = new List<Color> {Color.Red, Color.DarkOrange, Color.Red, Color.DarkOrange};
        }

        private void UpdateSmoke()
        {
            if (CsJson["player"]?["state"]?["smoked"] == null)
                return;

            var smoked = CsJson["player"]["state"]["smoked"].Value<int>();
            if (smoked == 0 && !DrawingSmoke)
                return;

            EventRect.Colors = new List<Color> {Color.FromArgb(smoked, 255, 255, 255)};
            DrawingSmoke = (smoked != 0);
        }

        private void UpdateFlash()
        {
            if (CsJson["player"]?["state"]?["flashed"] == null)
                return;

            var flashed = CsJson["player"]["state"]["flashed"].Value<int>();
            if (flashed == 0 && !DrawingFlash)
                return;

            EventRect.Colors = new List<Color> {Color.FromArgb(flashed, 255, 255, 255)};
            DrawingFlash = (flashed != 0);
        }

        private void UpdateTeam()
        {
            var currentTeam = CsJson["player"]?["team"];
            if (currentTeam == null)
                return;

            var t1 = Color.FromArgb(255, 255, 129, 0);
            var t2 = Color.FromArgb(255, 255, 89, 0);

            var ct1 = Color.FromArgb(255, 203, 238, 255);
            var ct2 = Color.FromArgb(255, 0, 173, 255);

            TeamRect.Colors = currentTeam.Value<string>() == "T"
                ? new List<Color> {t1, t2, t1, t2}
                : new List<Color> {ct1, ct2, ct1, ct2};
            TeamRect.Rotate = true;
        }

        private void UpdateAmmo()
        {
            if (CsJson["player"]["weapons"] == null)
                return;

            var activeWeapon =
                CsJson["player"]["weapons"].Children()
                    .Select(c => c.First)
                    .FirstOrDefault(w => w["state"]?.Value<string>() == "active");

            // Update the ammo display
            if (activeWeapon?["ammo_clip_max"] == null || activeWeapon["ammo_clip"] == null)
                return;

            var maxAmmo = activeWeapon["ammo_clip_max"].Value<int>();
            var ammo = activeWeapon["ammo_clip"].Value<int>();

            if (maxAmmo < 0)
                return;

            var ammoPercentage = (int) Math.Ceiling(100.00/maxAmmo)*ammo;
            AmmoRect.Width = ((int) Math.Floor((16/100.00)*ammoPercentage))*Scale;

            // Low ammo indicator
            if (ammoPercentage < 37)
                AmmoRect.StartBlink(1000);
            else
                AmmoRect.StopBlink();
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(21*Scale, 6*Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                AmmoRect.Draw(g);
                TeamRect.Draw(g);
                EventRect.Draw(g);
            }
            return bitmap;
        }

        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's CS:GO JSON
            if (!jsonString.Contains("Counter-Strike: Global Offensive"))
                return;


            Debug.WriteLine(jsonString);
            // Parse the JSON
            CsJson = JsonConvert.DeserializeObject<JObject>(jsonString);
        }
    }
}