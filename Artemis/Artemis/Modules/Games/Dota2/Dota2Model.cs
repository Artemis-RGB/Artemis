using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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
        private KeyboardRegion _keyPad;
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
        public KeyboardRectangle HealthRectangle { get; set; }
        public KeyboardRectangle EventRectangle { get; set; }
        public KeyboardRectangle DayCycleRectangle { get; set; }
        public KeyboardRectangle ManaRectangle { get; set; }
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
            _keyPad = MainManager.KeyboardManager.ActiveKeyboard.KeyboardRegions.First(r => r.RegionName == "NumPad");
            HealthRectangle = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard
                , 0
                , _topRow.TopLeft.X
                , new List<Color>()
                , LinearGradientMode.Horizontal)
            { Height = Scale, ContainedBrush = false };

            ManaRectangle = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard
                , 0
                , _topRow.TopLeft.X+1
                , new List<Color>()
                , LinearGradientMode.Horizontal)
            { Height = Scale, ContainedBrush = false };

            EventRectangle = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard
                , 0
                , _topRow.TopLeft.X + 2
                , new List<Color>()
                , LinearGradientMode.Horizontal )
            {Height = MainManager.KeyboardManager.ActiveKeyboard.Height*Scale - Scale
            , Width = MainManager.KeyboardManager.ActiveKeyboard.Width*Scale-Scale-12};

            DayCycleRectangle = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard
                , _keyPad.BottomRight.X * 3
                , _keyPad.TopLeft.X 
                , new List<Color>()
                , LinearGradientMode.Horizontal)
            {
                Height = MainManager.KeyboardManager.ActiveKeyboard.Height*Scale - Scale,
                Width = MainManager.KeyboardManager.ActiveKeyboard.Width - (MainManager.KeyboardManager.ActiveKeyboard.Width * Scale - Scale - 12)
            };
            MainManager.GameStateWebServer.GameDataReceived += HandleGameData;
            Initialized = true;
        }

        public override void Update()
        {
            if (D2Json == null)
                return;

            if (Settings.ShowDead)
                UpdateDead();
            if (Settings.CanCastAbility)
                UpdateAbilities();
            if (Settings.ShowHealth)
                UpdateHealth();
            if (Settings.ShowDayCycle)
                UpdateDay();
            if (Settings.ShowMana)
                UpdateMana();
            if (Settings.CanCastItem)
                UpdateItems();
           
        }

        private void UpdateDead()
        {
            if (D2Json?.hero?.alive == null)
                return;

            EventRectangle.Colors = D2Json.hero.alive ? new List<Color> { Color.Lime } : new List<Color> {Color.LightGray};
        }

        private void UpdateDay()
        {

            if (D2Json?.map?.daytime == null)
                return;

            if (D2Json.map.nightstalker_night)
            {
                DayCycleRectangle.Colors = new List<Color> { Color.Blue };
                return;
            }

            var timeLeft = 240 - (D2Json.map.clock_time % 240);
            double width = (int) ((MainManager.KeyboardManager.ActiveKeyboard.Width*Scale - (MainManager.KeyboardManager.ActiveKeyboard.Width * Scale - Scale*Scale)) * (timeLeft/240D)) ;
            DayCycleRectangle.Width = (int) (width/2) > 1 ? (int)(width / 2)+1 : (int)(width / 2);
            DayCycleRectangle.Colors = D2Json.map.daytime ? new List<Color> { Color.Yellow } : new List<Color> { Color.Blue };

        }

        private void UpdateMana()
        {
            if (D2Json?.hero == null || D2Json.hero.mana_percent == -1)
                return;

            var manaPercent = D2Json.hero.mana_percent;
            ManaRectangle.Colors = new List<Color> {Color.Blue};
            ManaRectangle.Width = (int)Math.Floor(_topRow.BottomRight.Y / 100.00 * manaPercent) * Scale;
        }

        private void UpdateItems()
        {
            //throw new NotImplementedException();
        }

        private void UpdateAbilities()
        {

            //Console.WriteLine();

            //Update keys according to the abilities they take.
        }


        private void UpdateHealth()
        {
            if (D2Json?.hero == null || D2Json.hero.health_percent == -1)
                return;

            var healthPercent = D2Json.hero.health_percent;
            if(healthPercent > 66)
                HealthRectangle.Colors = new List<Color> { Color.Lime };
            else if (healthPercent > 33)
                HealthRectangle.Colors = new List<Color> {Color.Yellow};
            else
                HealthRectangle.Colors = new List<Color> {Color.Red};

            HealthRectangle.Width = (int)Math.Floor(_topRow.BottomRight.Y / 100.00 * healthPercent) * Scale;

        }


        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                EventRectangle.Draw(g);
                HealthRectangle.Draw(g);
                ManaRectangle.Draw(g);
                DayCycleRectangle.Draw(g);
                
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
