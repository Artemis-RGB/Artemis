using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using RGB.NET.Core;
using Color = System.Drawing.Color;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule
    {
        private readonly ColorBlend _rainbowColorBlend;
        private readonly PluginSettings _settings;

        public GeneralModule(PluginInfo pluginInfo, PluginSettings settings, ISurfaceService surfaceService) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;
            DeviceBrushes = new Dictionary<ArtemisDevice, TextureBrush>();

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);

            Hues = new int[1000];
            for (var i = 0; i < 1000; i++)
                Hues[i] = ColorHelpers.GetRandomHue();

            _rainbowColorBlend = new ColorBlend(9);
            for (var i = 0; i < 9; i++)
            {
                _rainbowColorBlend.Positions[i] = i / 8f;
                if (i != 8)
                    _rainbowColorBlend.Colors[i] = ColorHelpers.ColorFromHSV(i * 32, 1, 1);
                else
                    _rainbowColorBlend.Colors[i] = ColorHelpers.ColorFromHSV(0, 1, 1);
            }

            surfaceService.SurfaceConfigurationUpdated += (sender, args) => DeviceBrushes.Clear();
        }

        public int[] Hues { get; set; }
        public int MovePercentage { get; set; }

        public Dictionary<ArtemisDevice, TextureBrush> DeviceBrushes { get; set; }

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

            MovePercentage++;
            if (MovePercentage > 100)
                MovePercentage = 0;

            base.Update(deltaTime);
        }


        public override void Render(double deltaTime, ArtemisSurface surface, Graphics graphics)
        {
            // Per-device coloring, slower
            // RenderPerDevice(surface, graphics);

            // Per-LED coloring, slowest
            // RenderPerLed(surface, graphics);

            base.Render(deltaTime, surface, graphics);
        }

        public void RenderFullSurface(ArtemisSurface surface, Graphics graphics)
        {
        }

        public void RenderPerDevice(ArtemisSurface surface, Graphics graphics)
        {
            foreach (var device in surface.Devices)
            {
                if (!DeviceBrushes.ContainsKey(device))
                    DeviceBrushes.Add(device, new TextureBrush(RenderGradientForDevice(device), WrapMode.Tile));

                var brush = DeviceBrushes[device];
                brush.TranslateTransform((int) Math.Round(device.RenderRectangle.Width / 100.0 * MovePercentage), 0);
                graphics.FillPath(brush, device.RenderPath);
                brush.TranslateTransform((int) Math.Round(device.RenderRectangle.Width / 100.0 * MovePercentage) * -1, 0);

                graphics.DrawRectangle(new Pen(Color.Red), device.RenderRectangle);
            }
        }

        private Image RenderGradientForDevice(ArtemisDevice device)
        {
            var brush = new LinearGradientBrush(device.RenderRectangle, Color.Black, Color.Black, 0, false)
            {
                InterpolationColors = _rainbowColorBlend
            };
            var bitmap = new Bitmap(device.RenderRectangle.Width, device.RenderRectangle.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(brush, 0, 0, device.RenderRectangle.Width, device.RenderRectangle.Height);
            }

            return bitmap;
        }

        public void RenderPerLed(ArtemisSurface surface, Graphics graphics)
        {
            var index = 0;
            foreach (var led in surface.Devices.SelectMany(d => d.Leds))
            {
                if (led.RgbLed.Id == LedId.HeadsetStand1)
                    graphics.FillRectangle(new SolidBrush(Color.Red), led.AbsoluteRenderRectangle);
                else
                    graphics.FillRectangle(new SolidBrush(ColorHelpers.ColorFromHSV(Hues[index], 1, 1)), led.AbsoluteRenderRectangle);
                index++;
            }
        }

        public override IEnumerable<ModuleViewModel> GetViewModels()
        {
            return new List<ModuleViewModel> {new GeneralViewModel(this)};
        }

        public override void Dispose()
        {
        }
    }
}