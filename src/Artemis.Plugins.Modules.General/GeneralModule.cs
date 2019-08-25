using System;
using System.Drawing;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using RGB.NET.Core;
using Stylet;
using Color = System.Drawing.Color;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : Module
    {
        private readonly PluginSettings _settings;
        private readonly RGBSurface _surface;
        private Color _color;

        public GeneralModule(PluginInfo pluginInfo, IRgbService rgbService, PluginSettings settings) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;

            _surface = rgbService.Surface;

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
            _color = ColorHelpers.GetRandomRainbowColor();
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            _color = ColorHelpers.ShiftColor(_color, (int) (deltaTime * 200));
        }

        public override void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            // Lets do this in the least performant way possible
            foreach (var surfaceLed in _surface.Leds)
            {
                var rectangle = surfaceLed.AbsoluteLedRectangle.ToDrawingRectangle();
                graphics.FillRectangle(new SolidBrush(_color), rectangle);
            }
        }

        public override IScreen GetMainViewModel()
        {
            return new GeneralViewModel(this);
        }

        public override void Dispose()
        {
        }
    }
}