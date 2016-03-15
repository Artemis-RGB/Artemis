using System;
using System.Diagnostics;
using System.Drawing;
using Artemis.KeyboardProviders.Logitech.Utilities;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.LogitechDll;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionModel : GameModel
    {
        public TheDivisionModel(MainManager mainManager, TheDivisionSettings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "TheDivision";
            ProcessName = "TheDivision";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public TheDivisionSettings Settings { get; set; }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            DllManager.RestoreDll();
        }

        public override void Enable()
        {
            Initialized = false;
            DllManager.PlaceDll();
            MainManager.PipeServer.PipeMessage += PipeServerOnPipeMessage;
            Initialized = true;
        }

        private void PipeServerOnPipeMessage(string reply)
        {
            // Convert the given string to a list of ints
            var stringParts = reply.Split(' ');
            var parts = new int[stringParts.Length];
            for (var i = 0; i < stringParts.Length; i++)
                parts[i] = int.Parse(stringParts[i]);

            if (parts[0] == 1)
                InterpertrateDivisionKey(parts);

        }

        // Parses Division key data to game data
        private void InterpertrateDivisionKey(int[] parts)
        {
            // F1 to F4 indicate the player and his party. Blinks red on damage taken
            
            // R blinks white when low on ammo

            // G turns white when holding a grenade, turns off when out of grenades

            // V blinks on low HP
        }

        public override void Update()
        {
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
            }
            return bitmap;
        }
    }
}