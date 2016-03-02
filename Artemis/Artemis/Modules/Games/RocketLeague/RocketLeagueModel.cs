using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : GameModel
    {
        private int _boostAmount;
        private bool _boostGrowing;
        private KeyboardRectangle _boostRect;
        private bool _contextualColor;
        private Memory _memory;
        private GamePointersCollection _pointer;
        private int _previousBoost;

        public RocketLeagueModel(MainManager mainManager, RocketLeagueSettings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public RocketLeagueSettings Settings { get; set; }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _memory = null;
        }

        public override void Enable()
        {
            Initialized = false;

            _boostRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>
            {
                ColorHelpers.ToDrawingColor(Settings.MainColor),
                ColorHelpers.ToDrawingColor(Settings.SecondaryColor)
            }, LinearGradientMode.Horizontal);

            Updater.GetPointers();
            _pointer = JsonConvert.DeserializeObject<GamePointersCollection>(Offsets.Default.RocketLeague);

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            _memory = new Memory(tempProcess);

            Initialized = true;
        }

        public override void Update()
        {
            if (_boostGrowing)
                return;
            if (_memory == null)
                return;

            ContextualColor = Settings.ContextualColor;
            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostFloat = _memory.ReadFloat(boostAddress)*100/3;

            _previousBoost = _boostAmount;
            _boostAmount = (int) Math.Ceiling(boostFloat);

            // Take care of any reading errors resulting in an OutOfMemory on draw
            if (_boostAmount < 0)
                _boostAmount = 0;
            if (_boostAmount > 100)
                _boostAmount = 100;

            _boostRect.Width =
                (int) Math.Ceiling(MainManager.KeyboardManager.ActiveKeyboard.Width*Scale/100.00*_boostAmount);

            if (_contextualColor)
            {
                if (_boostAmount < 33)
                    _boostRect.Colors = new List<Color> {Color.Red};
                else if (_boostAmount >= 33 && _boostAmount < 66)
                    _boostRect.Colors = new List<Color> {Color.Yellow};
                else if (_boostAmount >= 66)
                    _boostRect.Colors = new List<Color> {Color.Lime};
            }

            Task.Run(() => GrowIfHigher());
        }

        private void GrowIfHigher()
        {
            if (_boostAmount <= _previousBoost || _boostGrowing)
                return;

            _boostGrowing = true;
            const int amountOfSteps = 6;

            var difference = _boostAmount - _previousBoost;
            var differenceStep = difference/amountOfSteps;
            var differenceStepRest = difference%amountOfSteps;
            _boostAmount = _previousBoost;
            _boostRect.Width =
                (int) Math.Ceiling(MainManager.KeyboardManager.ActiveKeyboard.Width*Scale/100.00*_boostAmount);

            for (var i = 0; i < amountOfSteps; i++)
            {
                if (differenceStepRest > 0)
                {
                    differenceStepRest -= 1;
                    _boostAmount += 1;
                    _boostRect.Width =
                        (int) Math.Ceiling(MainManager.KeyboardManager.ActiveKeyboard.Width*Scale/100.00*_boostAmount);
                }
                _boostAmount += differenceStep;
                _boostRect.Width =
                    (int) Math.Ceiling(MainManager.KeyboardManager.ActiveKeyboard.Width*Scale/100.00*_boostAmount);

                Thread.Sleep(50);
            }

            _boostGrowing = false;
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);
            if (_boostRect == null)
                return null;

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                _boostRect.Draw(g);
            }
            return bitmap;
        }
    }
}