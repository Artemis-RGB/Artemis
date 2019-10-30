using System;
using System.Drawing;
using System.Linq;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.General.ViewModels;
using Stylet;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : Module
    {
        private readonly PluginSettings _settings;

        public GeneralModule(PluginInfo pluginInfo, PluginSettings settings, ISettingsService settingsService) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
            Colors = new Color[1000];
        }

        public Color[] Colors { get; set; }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            for (var i = 0; i < Colors.Length; i++)
            {
                var color = Colors[i];
                Colors[i] = ColorHelpers.ShiftColor(color, (int) (deltaTime * 200));
            }
        }

        public override void Render(double deltaTime, Surface surface, Graphics graphics)
        {
            // Per-device coloring, slower
            // RenderPerDevice(surface, graphics);

            // Per-LED coloring, slowest
            RenderPerLed(surface, graphics);
        }

        public void RenderFullSurface(Surface surface, Graphics graphics)
        {
        }

        public void RenderPerDevice(Surface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var device in surface.Devices)
            {
                var color = Colors[index];
                if (color.A == 0)
                {
                    color = ColorHelpers.GetRandomRainbowColor();
                    Colors[index] = color;
                }

                graphics.FillRectangle(new SolidBrush(color), device.RenderRectangle);
                index++;
            }
        }

        public void RenderPerLed(Surface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var led in surface.Devices.SelectMany(d => d.Leds))
            {
                var color = Colors[index];
                if (color.A == 0)
                {
                    color = ColorHelpers.GetRandomRainbowColor();
                    Colors[index] = color;
                }

                graphics.FillRectangle(new SolidBrush(color), led.AbsoluteRenderRectangle);
                index++;
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