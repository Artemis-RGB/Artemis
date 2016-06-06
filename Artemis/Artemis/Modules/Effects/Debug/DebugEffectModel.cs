using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Utilities.Keyboard;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Effects.Debug
{
    // TODO: Remove
    internal class DebugEffectModel : EffectModel
    {
        public DebugEffectModel(MainManager mainManager, DebugEffectSettings settings) : base(mainManager, null)
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

            KeyboardRectangle = new KeyboardRectangle(MainManager.DeviceManager.ActiveKeyboard, 0, 0, new List<Color>
            {
                Color.FromArgb(0, 226, 190),
                Color.FromArgb(0, 208, 255)
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

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return null;
        }

        public override void Render(out Bitmap keyboard, out Brush mouse, out Brush headset, bool renderMice, bool renderHeadsets)
        {
            mouse = null;
            headset = null;

            keyboard = MainManager.DeviceManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(keyboard))
            {
                g.Clear(Color.Transparent);
                KeyboardRectangle.Draw(g);
            }
        }
    }
}