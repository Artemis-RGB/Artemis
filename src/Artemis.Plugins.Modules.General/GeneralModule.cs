using System;
using System.Drawing;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.Modules.General.ViewModels;
using RGB.NET.Core;
using Stylet;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

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

        public override void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            // Per-device coloring, slower
            // RenderPerDevice(surface, graphics);

            // Per-LED coloring, slowest
            RenderPerLed(surface, graphics);
        }

        public void RenderFullSurface(RGBSurface surface, Graphics graphics)
        {
        }

        public void RenderPerDevice(RGBSurface surface, Graphics graphics)
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

                var rectangle = new Rectangle((int) device.Location.X, (int) device.Location.Y, (int) device.Size.Width, (int) device.Size.Height);
                graphics.FillRectangle(new SolidBrush(color), rectangle);
                index++;
            }
        }

        public void RenderPerLed(RGBSurface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var led in surface.Leds)
            {
                var color = Colors[index];
                if (color.A == 0)
                {
                    color = ColorHelpers.GetRandomRainbowColor();
                    Colors[index] = color;
                }

                var rectangle = led.AbsoluteLedRectangle.ToDrawingRectangle();
                graphics.FillRectangle(new SolidBrush(color), rectangle);
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