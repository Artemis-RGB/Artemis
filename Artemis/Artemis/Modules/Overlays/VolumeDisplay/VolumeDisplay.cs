using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplay
    {
        public VolumeDisplay()
        {
            Transparancy = 255;
            Scale = 4;
        }

        public int Scale { get; set; }

        public int Ttl { get; set; }
        public byte Transparancy { get; set; }
        public int Volume { get; set; }


        public void Draw(Graphics g)
        {
            var mainColor = Color.FromArgb(Transparancy, 0, 191, 255);
            var secondaryColor = Color.FromArgb(Transparancy, 255, 0, 0);
            var volumeRect = new KeyboardRectangle(Scale, 0, 0, (int) Math.Ceiling(((Scale*21)/100.00)*Volume),
                8*Scale, new List<Color> {mainColor, secondaryColor}, LinearGradientMode.Horizontal);
            volumeRect.Draw(g);
        }
    }
}