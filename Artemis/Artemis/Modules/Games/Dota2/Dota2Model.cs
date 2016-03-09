using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.KeyboardProviders;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.GameState;
using Artemis.Utilities.Keyboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Artemis.Modules.Games.Dota2;

namespace Artemis.Modules.Games.Dota2
{
    class Dota2Model : GameModel
    {

        private KeyboardRegion _topRow;
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
        public Dota2DataModel.Rootobject D2Json { get; set; }
        public int Scale { get; set; }
        public KeyboardRectangle HealthRect { get; set; }

        #endregion


        public override void Dispose()
        {
            Initialized = false;
            MainManager.GameStateWebServer.GameDataReceived -= HandleGameData;
        }

        public override void Enable()
        {
            Initialized = false;
            _topRow = MainManager.KeyboardManager.ActiveKeyboard.KeyboardRegions.First(r => r.RegionName == "TopRow");
            HealthRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, _topRow.TopLeft.X,
                new List<Color>(),
                LinearGradientMode.Horizontal)
            { Height = Scale, ContainedBrush = false };
            MainManager.GameStateWebServer.GameDataReceived += HandleGameData;
            Initialized = true;
        }

        public override void Update()
        {
            if (D2Json == null)
                return;

            if (Settings.CanCastAbility)
                UpdateAbilities();
            if (Settings.ShowHealth)
                UpdateHealth();
            /*
            * if (Settings.ShowDayCycle)
            *   UpdateDay();
            * if (Settings.ShowMana) //Not sure if this is in the Json. Will check when I get home
            *   UpdateMana();
            * if (Settings.CanCastItems)
            *   UpdateItems();
            * if (Settings.) //Add something
            */
        }

        private void UpdateHealth()
        {
            var health = D2Json.hero.health_percent;
            if(health > 66)
                HealthRect.Colors = new List<Color> { Color.Lime };
            else if (health > 33)
                HealthRect.Colors = new List<Color> {Color.Yellow};
            else
                HealthRect.Colors = new List<Color> {Color.Red};
            
        }

        private void UpdateAbilities()
        {

            Console.WriteLine();

            //Update keys according to the abilities they take.
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                HealthRect.Draw(g);
            }
            return bitmap;
        }



        public void HandleGameData(object sender, GameDataReceivedEventArgs e)
        {
            var jsonString = e.Json.ToString();

            // Ensure it's Dota 2 JSON
            if (!jsonString.Contains("Dota 2"))
                return;

            // Parse the JSON
            D2Json = JsonConvert.DeserializeObject<Dota2DataModel.Rootobject>(jsonString);
        }
    }
}
