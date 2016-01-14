using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Models;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;
using MyMemory;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : GameModel
    {
        private readonly KeyboardRectangle _boostRect;

        private int _boostAmount;
        private bool _boostGrowing;
        private int _previousBoost;

        private Memory _memory;
        private GamePointersCollectionModel _pointer;

        public RocketLeagueModel(RocketLeagueSettings settings)
        {
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;

            _boostRect = new KeyboardRectangle(Scale, 0, 0, Scale*21, Scale*8,
                new List<Color>
                {
                    ColorHelpers.MediaColorToDrawingColor(settings.MainColor),
                    ColorHelpers.MediaColorToDrawingColor(settings.SecondaryColor)
                }, LinearGradientMode.Horizontal);

            Enabled = settings.Enabled;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            _memory = null;
        }

        public override void Enable()
        {
            MemoryHelpers.GetPointers();
            _pointer = JsonConvert
                .DeserializeObject<GamePointersCollectionModel>(Offsets.Default.RocketLeague);

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            _memory = new Memory(tempProcess);
        }

        public override void Update()
        {
            if (_boostGrowing)
                return;
            if (_memory == null)
                return;

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

            _boostRect.Width = (int) Math.Ceiling(((Scale*21)/100.00)*_boostAmount);

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
            _boostRect.Width = (int) Math.Ceiling(((Scale*21)/100.00)*_boostAmount);

            for (var i = 0; i < amountOfSteps; i++)
            {
                if (differenceStepRest > 0)
                {
                    differenceStepRest -= 1;
                    _boostAmount += 1;
                    _boostRect.Width = (int) Math.Ceiling(((Scale*21)/100.00)*_boostAmount);
                }
                _boostAmount += differenceStep;
                _boostRect.Width = (int) Math.Ceiling(((Scale*21)/100.00)*_boostAmount);

                Thread.Sleep(50);
            }

            _boostGrowing = false;
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(Scale*21, Scale*8);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                _boostRect.Draw(g);
            }
            return bitmap;
        }
    }
}