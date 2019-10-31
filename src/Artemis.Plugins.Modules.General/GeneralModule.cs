using System;
using System.Drawing;
using System.Linq;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.Modules.General.ViewModels;
using Stylet;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : Module
    {
        private readonly PluginSettings _settings;

        public GeneralModule(PluginInfo pluginInfo, PluginSettings settings) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);

            Hues = new int[1000];
            for (var i = 0; i < 1000; i++)
                Hues[i] = ColorHelpers.GetRandomHue();
        }

        public int[] Hues { get; set; }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            for (var i = 0; i < Hues.Length; i++)
            {
                Hues[i]++;
                if (Hues[i] > 360)
                    Hues[i] = 0;
            }
        }

        public override void Render(double deltaTime, Surface surface, Graphics graphics)
        {
            // Per-device coloring, slower
            RenderPerDevice(surface, graphics);

            // Per-LED coloring, slowest
            // RenderPerLed(surface, graphics);
        }

        public void RenderFullSurface(Surface surface, Graphics graphics)
        {
        }

        public void RenderPerDevice(Surface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var device in surface.Devices)
            {
                graphics.FillRectangle(new SolidBrush(ColorHelpers.ColorFromHSV(Hues[index], 1, 1)), device.RenderRectangle);
                index++;
            }
        }

        public void RenderPerLed(Surface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var led in surface.Devices.SelectMany(d => d.Leds))
            {
                graphics.FillRectangle(new SolidBrush(ColorHelpers.ColorFromHSV(Hues[index], 1, 1)), led.AbsoluteRenderRectangle);
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