//css_inc GeneralViewModel.cs;
//css_inc GeneralDataModel.cs;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralModule : IModule
    {
        private readonly IRgbService _rgbService;
        private readonly RGBSurface _surface;
        private Dictionary<Led, Color> _colors;

        public GeneralModule(IRgbService rgbService)
        {
            _rgbService = rgbService;
            _surface = _rgbService.Surface;
            _colors = new Dictionary<Led, Color>();

            _rgbService.FinishedLoadedDevices += (sender, args) => PopulateColors();
        }

        public Type ViewModelType
        {
            get { return typeof(GeneralViewModel); }
        }

        // True since the main data model is all this module shows
        public bool ExpandsMainDataModel
        {
            get { return true; }
        }

        public void Update(double deltaTime)
        {
            if (_colors == null)
                return;

            foreach (var surfaceLed in _surface.Leds)
                UpdateLedColor(surfaceLed, deltaTime);

            
        }

        private void UpdateLedColor(Led led, double deltaTime)
        {
            if (_colors.ContainsKey(led))
                _colors[led] = ColorHelpers.ShiftColor(_colors[led], (int) (deltaTime * 1000));
            else
                _colors[led] = ColorHelpers.GetRandomRainbowColor();
        }

        public void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            foreach (var surfaceLed in _surface.Leds)
            {
                if (!_colors.ContainsKey(surfaceLed))
                    continue;

                var brush = new SolidBrush(_colors[surfaceLed]);
                var rectangle = new Rectangle((int) surfaceLed.LedRectangle.X, (int) surfaceLed.LedRectangle.Y, (int) surfaceLed.LedRectangle.Width, (int) surfaceLed.LedRectangle.Height);
                graphics.FillRectangle(brush, rectangle);
                UpdateLedColor(surfaceLed, deltaTime);
            }
        }

        public void Dispose()
        {
            _colors = null;
        }

        public void LoadPlugin()
        {
            PopulateColors();
        }

        private void PopulateColors()
        {
            _colors = new Dictionary<Led, Color>();
            foreach (var surfaceLed in _surface.Leds)
                _colors.Add(surfaceLed, ColorHelpers.GetRandomRainbowColor());
        }
    }
}