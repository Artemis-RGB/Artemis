using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Models;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;
using MyMemory;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3Model : GameModel
    {
        public Witcher3Model(RocketLeagueSettings settings)
        {
            Name = "Witcher3";
            ProcessName = "witcher3";
            Scale = 4;

            Settings = settings;
            SignRect = new KeyboardRectangle(Scale, 0, 0, 84, 24, new List<Color> {Color.Blue, Color.Red},
                LinearGradientMode.Horizontal)
            {
                Rotate = true,
                LoopSpeed = 0.5
            };

            Enabled = Settings.Enabled;
        }

        public int Scale { get; set; }
        public RocketLeagueSettings Settings { get; set; }

        public KeyboardRectangle SignRect { get; set; }

        private RemoteProcess Process { get; set; }
        private Memory Memory { get; set; }

        public IntPtr BaseAddress { get; set; }

        public override void Dispose()
        {
            Process = null;
            Memory = null;
        }

        public override void Enable()
        {
            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            BaseAddress = tempProcess.MainModule.BaseAddress;
            Process = new RemoteProcess((uint) tempProcess.Id);
        }

        public override void Update()
        {
            if (Process == null)
                return;

            // TODO: Get address from web on startup
            var processHandle = Process.ProcessHandle;
            var baseAddress = (IntPtr) 0x028F3F60;
            int[] offsets = {0x28, 0x10, 0x20, 0xbc0};

            var addr = MemoryHelpers.FindAddress(processHandle, BaseAddress, baseAddress, offsets);
            var result = Process.MemoryManager.Read<byte>(addr);

            switch (result)
            {
                case 0:
                    // Aard
                    SignRect.Colors = new List<Color> {Color.DeepSkyBlue, Color.Blue, Color.DeepSkyBlue, Color.Blue};
                    break;
                case 1:
                    // Yrden
                    SignRect.Colors = new List<Color> {Color.Purple, Color.DeepPink, Color.Purple, Color.DeepPink};
                    break;
                case 2:
                    // Igni
                    SignRect.Colors = new List<Color> {Color.DarkOrange, Color.Red, Color.DarkOrange, Color.Red};
                    break;
                case 3:
                    // Quen
                    SignRect.Colors = new List<Color> {Color.DarkOrange, Color.Yellow, Color.DarkOrange, Color.Yellow};
                    break;
                case 4:
                    // Axii
                    SignRect.Colors = new List<Color>
                    {
                        Color.LawnGreen,
                        Color.DarkGreen,
                        Color.LawnGreen,
                        Color.DarkGreen
                    };
                    break;
            }
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(21, 6);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                SignRect.Draw(g);
            }
            return bitmap;
        }
    }
}