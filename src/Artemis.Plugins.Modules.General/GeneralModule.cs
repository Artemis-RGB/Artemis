using System.Collections.Generic;
using System.Drawing;
using Artemis.Core;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using QRCoder;
using RGB.NET.Core;
using Stylet;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : IModule
    {
        private readonly RGBSurface _surface;
        private Dictionary<Led, Color> _colors;

        public GeneralModule(IRgbService rgbService)
        {
            var rgbService1 = rgbService;
            _surface = rgbService1.Surface;
            _colors = new Dictionary<Led, Color>();

            rgbService1.FinishedLoadedDevices += (sender, args) => PopulateColors();
        }

        public string DisplayName => "General";

        // True since the main data model is all this module shows
        public bool ExpandsMainDataModel => true;

        public void Update(double deltaTime)
        {
            if (_colors == null)
                return;

            foreach (var surfaceLed in _surface.Leds)
                UpdateLedColor(surfaceLed, deltaTime);
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

        public IScreen GetMainViewModel()
        {
            return new GeneralViewModel(this);
        }

        public void Dispose()
        {
            _colors = null;
        }

        public void EnablePlugin()
        {
            var qrGenerator = new QRCodeGenerator();
            PopulateColors();
        }

        private void UpdateLedColor(Led led, double deltaTime)
        {
            if (_colors.ContainsKey(led))
                _colors[led] = ColorHelpers.ShiftColor(_colors[led], (int) (deltaTime * 1000));
            else
                _colors[led] = ColorHelpers.GetRandomRainbowColor();
        }

        private void PopulateColors()
        {
            _colors = new Dictionary<Led, Color>();
            foreach (var surfaceLed in _surface.Leds)
                _colors.Add(surfaceLed, ColorHelpers.GetRandomRainbowColor());
        }
    }
}