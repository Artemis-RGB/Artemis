using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Effects.Debug
{
    // TODO: Remove
    internal class DebugEffectModel : EffectModel
    {
        public DebugEffectModel(MainManager mainManager, KeyboardManager keyboardManager, DebugEffectSettings settings)
            : base(mainManager, keyboardManager)
        {
            Name = "Debug Effect";
            Settings = settings;
            Scale = 4;
            Initialized = false;
        }

        public int Scale { get; set; }

        public DebugEffectSettings Settings { get; set; }

        public KeyboardRectangle KeyboardRectangle { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            Initialized = false;

            KeyboardRectangle = new KeyboardRectangle(KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>
            {
                Color.Red,
                Color.OrangeRed,
                Color.Yellow,
                Color.Green,
                Color.Blue,
                Color.Purple,
                Color.DeepPink
            }, LinearGradientMode.Horizontal);

            Initialized = true;
        }

        public override void Update()
        {
            KeyboardRectangle.Height = Settings.Height;
            KeyboardRectangle.Width = Settings.Width;
            KeyboardRectangle.GradientMode = Settings.Type;
            KeyboardRectangle.Rotate = Settings.Rotate;
            KeyboardRectangle.Scale = Settings.Scale;
            Scale = Settings.Scale;
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(21*Scale, 6*Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                KeyboardRectangle.Draw(g);
            }
            return bitmap;
        }
    }
}