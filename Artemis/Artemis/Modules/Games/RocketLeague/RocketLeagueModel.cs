using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Models;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : GameModel
    {
        public RocketLeagueModel(RocketLeagueSettings settings)
        {
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;

            Settings = settings;
            BoostRectangle = new KeyboardRectangle(Scale, 0, 0, Scale*21, Scale*8,
                new List<Color>
                {
                    ColorHelpers.MediaColorToDrawingColor(Settings.MainColor),
                    ColorHelpers.MediaColorToDrawingColor(Settings.SecondaryColor)
                }, LinearGradientMode.Horizontal);

            Enabled = Settings.Enabled;
        }

        public int Scale { get; set; }

        public RocketLeagueSettings Settings { get; set; }

        private int BoostAmount { get; set; }
        private KeyboardRectangle BoostRectangle { get; }

        private Process Process { get; set; }

        private int PreviousBoost { get; set; }


        private bool BoostGrowing { get; set; }

        private Memory Memory { get; set; }

        public override void Dispose()
        {
            Process = null;
            Memory = null;
        }

        public override void Enable()
        {
            Process = MemoryHelpers.GetProcessIfRunning(ProcessName);
            Memory = new Memory(Process);
        }

        public override void Update()
        {
            if (BoostGrowing)
                return;
            if (Memory == null)
                return;

            // TODO: Get address from web on startup
            var boostAddress = Memory.GetAddress("\"RocketLeague.exe\"+014FAA04+58+5ac+6f4+21c");
            var boostFloat = Memory.ReadFloat(boostAddress)*100/3;

            PreviousBoost = BoostAmount;
            BoostAmount = (int) Math.Ceiling(boostFloat);

            // Take care of any reading errors resulting in an OutOfMemory on draw
            if (BoostAmount < 0)
                BoostAmount = 0;
            if (BoostAmount > 100)
                BoostAmount = 100;

            BoostRectangle.Width = (int) Math.Ceiling(((Scale*21)/100.00)*BoostAmount);

            Task.Run(() => GrowIfHigher());
        }

        private void GrowIfHigher()
        {
            if (BoostAmount <= PreviousBoost || BoostGrowing)
                return;

            BoostGrowing = true;
            const int amountOfSteps = 6;

            var difference = BoostAmount - PreviousBoost;
            var differenceStep = difference/amountOfSteps;
            var differenceStepRest = difference%amountOfSteps;
            BoostAmount = PreviousBoost;
            BoostRectangle.Width = (int) Math.Ceiling(((Scale*21)/100.00)*BoostAmount);

            for (var i = 0; i < amountOfSteps; i++)
            {
                if (differenceStepRest > 0)
                {
                    differenceStepRest -= 1;
                    BoostAmount += 1;
                    BoostRectangle.Width = (int) Math.Ceiling(((Scale*21)/100.00)*BoostAmount);
                }
                BoostAmount += differenceStep;
                BoostRectangle.Width = (int) Math.Ceiling(((Scale*21)/100.00)*BoostAmount);

                Thread.Sleep(50);
            }

            BoostGrowing = false;
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(Scale*21, Scale*8);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                BoostRectangle.Draw(g);
            }
            return bitmap;
        }
    }
}