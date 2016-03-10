using System.Drawing;
using Artemis.Managers;
using Artemis.Models;

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
        }

        public override void Enable()
        {
            Initialized = false;

            // Enable logic, if any

            Initialized = true;
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