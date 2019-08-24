using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using QRCoder;
using RGB.NET.Core;
using Stylet;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : Module
    {
        private readonly PluginSettings _settings;
        private readonly RGBSurface _surface;
        private Dictionary<Led, Color> _colors;
        private double _circlePosition;
        private Color _color;

        public GeneralModule(PluginInfo pluginInfo, IRgbService rgbService, PluginSettings settings) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;

            _surface = rgbService.Surface;
            _colors = new Dictionary<Led, Color>();

            rgbService.DeviceLoaded += (sender, args) => PopulateColors();
            rgbService.DeviceReloaded += (sender, args) => PopulateColors();

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
            _color = ColorHelpers.GetRandomRainbowColor();
        }

        public override void EnablePlugin()
        {
            var qrGenerator = new QRCodeGenerator();
            PopulateColors();
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            if (_colors == null)
                return;

            foreach (var surfaceLed in _surface.Leds)
                UpdateLedColor(surfaceLed, deltaTime);
        }

        public override void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            _circlePosition += deltaTime * 50;
            if (_circlePosition > 600)
            {
                _circlePosition = -200;
                _color = ColorHelpers.GetRandomRainbowColor();
            }

            var rect = new Rectangle((int) _circlePosition * 4, 0, 200, (int) surface.Leds.Max(l => l.AbsoluteLedRectangle.Y + l.AbsoluteLedRectangle.Height));
            graphics.FillRectangle(new SolidBrush(_color), rect);

//            // Lets do this in the least performant way possible
//            foreach (var surfaceLed in _surface.Leds)
//            {
//                var rectangle = surfaceLed.AbsoluteLedRectangle.ToDrawingRectangle();
//                if (surfaceLed.Id == LedId.Fan1)
//                {
//                    graphics.FillRectangle(new SolidBrush(Color.Red), rectangle);
//                }
//                if (surfaceLed.Id == LedId.Fan2)
//                {
//                    graphics.FillRectangle(new SolidBrush(Color.Blue), rectangle);
//                }
//                if (surfaceLed.Id == LedId.Fan3)
//                {
//                    graphics.FillRectangle(new SolidBrush(Color.Green), rectangle);
//                }
//            }


            return;

            // Lets do this in the least performant way possible
            foreach (var surfaceLed in _surface.Leds)
            {
                if (!_colors.ContainsKey(surfaceLed))
                    continue;

                var brush = new SolidBrush(_colors[surfaceLed]);
                var rectangle = surfaceLed.AbsoluteLedRectangle.ToDrawingRectangle();
                graphics.FillRectangle(brush, rectangle);
                UpdateLedColor(surfaceLed, deltaTime);
            }
        }

        public override IScreen GetMainViewModel()
        {
            return new GeneralViewModel(this);
        }

        public override void Dispose()
        {
            _colors = null;
        }

        private void UpdateLedColor(Led led, double deltaTime)
        {
            if (_colors.ContainsKey(led))
                _colors[led] = ColorHelpers.ShiftColor(_colors[led], (int) (deltaTime * 200));
            else
                _colors[led] = ColorHelpers.GetRandomRainbowColor();
        }

        private void PopulateColors()
        {
            lock (_colors)
            {
                _colors = new Dictionary<Led, Color>();
                foreach (var surfaceLed in _surface.Leds)
                    _colors.Add(surfaceLed, ColorHelpers.GetRandomRainbowColor());
            }
        }
    }
}