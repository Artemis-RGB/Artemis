using System;
using System.Collections.Generic;
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
            TeamRect = new KeyboardRectangle(Scale, 0, 1*Scale, 21*Scale, 8*Scale, new List<Color> {Color.CornflowerBlue},
                LinearGradientMode.Horizontal);
        }

        public KeyboardRectangle TeamRect { get; set; }

        public int Scale { get; set; }

        public JObject CsJson { get; set; }
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
            if (CsJson == null)
                return;

            UpdateAmmo();
            UpdateTeam();
        }

        private void UpdateTeam()
        {
            var currentTeam = CsJson["player"]["team"];
            if (currentTeam == null)
                return;

            TeamRect.Colors = currentTeam.Value<string>() == "T"
                ? new List<Color> {Color.FromArgb(255, 255, 129, 0)}
                : new List<Color> {Color.FromArgb(255, 112, 209, 255)};
        }

        private void UpdateAmmo()
        {
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
            CsJson = JsonConvert.DeserializeObject<JObject>(jsonString);
        }
    }
}