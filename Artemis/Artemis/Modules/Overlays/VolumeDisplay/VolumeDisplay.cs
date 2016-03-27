using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Managers;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeBar
    {
        public VolumeBar(MainManager mainManager, VolumeDisplaySettings settings)
        {
            MainManager = mainManager;
            Settings = settings;
            Transparancy = 255;
            Scale = 4;
        }

        public MainManager MainManager { get; set; }

        public VolumeDisplaySettings Settings { get; set; }

        public int Scale { get; set; }

        public int Ttl { get; set; }
        public byte Transparancy { get; set; }
        public int Volume { get; set; }


        public void Draw(Graphics g)
        {
            var volumeRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>
            {
                ColorHelpers.ToDrawingColor(Settings.MainColor),
                ColorHelpers.ToDrawingColor(Settings.SecondaryColor)
            },
                LinearGradientMode.Horizontal)
            {
                Width = (int) (MainManager.KeyboardManager.ActiveKeyboard.Width*Scale/100.00*Volume),
                ContainedBrush = false
            };
            volumeRect.Draw(g);
        }
    }
}