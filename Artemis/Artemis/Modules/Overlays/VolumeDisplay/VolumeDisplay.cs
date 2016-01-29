using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Models;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplay
    {
        public VolumeDisplay(MainModel mainModel, VolumeDisplaySettings settings)
        {
            MainModel = mainModel;
            Settings = settings;
            Transparancy = 255;
            Scale = 4;
        }

        public MainModel MainModel { get; set; }

        public VolumeDisplaySettings Settings { get; set; }

        public int Scale { get; set; }

        public int Ttl { get; set; }
        public byte Transparancy { get; set; }
        public int Volume { get; set; }


        public void Draw(Graphics g)
        {
            var volumeRect = new KeyboardRectangle(MainModel.ActiveKeyboard, 0, 0, new List<Color>
                {
                    ColorHelpers.MediaColorToDrawingColor(Settings.MainColor),
                    ColorHelpers.MediaColorToDrawingColor(Settings.SecondaryColor)
                },
                LinearGradientMode.Horizontal);
            volumeRect.Draw(g);
        }
    }
}