using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Models;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Settings;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.Memory;
using Binarysharp.MemoryManagement;
using MyMemory;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3Model : GameModel
    {
        private IntPtr _baseAddress;
        private GamePointersCollectionModel _pointer;
        private RemoteProcess _process;
        private KeyboardRectangle _signRect;
        private MemorySharp _mem;

        public Witcher3Model(MainModel mainModel, RocketLeagueSettings settings) : base(mainModel)
        {
            Name = "Witcher3";
            ProcessName = "witcher3";
            Scale = 4;
        }

        public int Scale { get; set; }

        public override bool Enabled()
        {
            return true;
        }

        public override void Dispose()
        {
            _process = null;
        }

        public override void Enable()
        {
            _signRect = new KeyboardRectangle(MainModel.ActiveKeyboard, 0, 0, new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Rotate = true,
                LoopSpeed = 0.5
            };
            MemoryHelpers.GetPointers();
            _pointer = JsonConvert
                .DeserializeObject<GamePointersCollectionModel>(Offsets.Default.Witcher3);

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            _baseAddress = tempProcess.MainModule.BaseAddress;
            _process = new RemoteProcess((uint) tempProcess.Id);
            _mem = new MemorySharp(tempProcess);
        }

        public override void Update()
        {
            if (_process == null)
                return;

            var processHandle = _process.ProcessHandle;
            var addr = MemoryHelpers.FindAddress(processHandle, _baseAddress,
                _pointer.GameAddresses.First(ga => ga.Description == "Sign").BasePointer,
                _pointer.GameAddresses.First(ga => ga.Description == "Sign").Offsets);

            var test =
                _mem.Modules.MainModule.FindPattern(
                    new byte[]
                    {
                        0x88, 0x07, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xC2, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0x48
                    },
                    "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", 0, true);
            var tessst = _process.MemoryManager.Read<byte>(test.Address);
            var result = _process.MemoryManager.Read<byte>(addr);

            switch (result)
            {
                case 0:
                    // Aard
                    _signRect.Colors = new List<Color>
                    {
                        Color.DeepSkyBlue,
                        Color.Blue,
                        Color.DeepSkyBlue,
                        Color.Blue
                    };
                    break;
                case 1:
                    // Yrden
                    _signRect.Colors = new List<Color>
                    {
                        Color.Purple,
                        Color.DeepPink,
                        Color.Purple,
                        Color.DeepPink
                    };
                    break;
                case 2:
                    // Igni
                    _signRect.Colors = new List<Color>
                    {
                        Color.DarkOrange,
                        Color.Red,
                        Color.DarkOrange,
                        Color.Red
                    };
                    break;
                case 3:
                    // Quen
                    _signRect.Colors = new List<Color>
                    {
                        Color.DarkOrange,
                        Color.Yellow,
                        Color.DarkOrange,
                        Color.Yellow
                    };
                    break;
                case 4:
                    // Axii
                    _signRect.Colors = new List<Color>
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
            var bitmap = MainModel.ActiveKeyboard.KeyboardBitmap();
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                _signRect.Draw(g);
            }
            return bitmap;
        }
    }
}