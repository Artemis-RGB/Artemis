using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using RGB.NET.Core;
using SkiaSharp;
using Color = RGB.NET.Core.Color;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule
    {
        private readonly PluginSettings _settings;

        public GeneralModule(PluginInfo pluginInfo, PluginSettings settings, ISurfaceService surfaceService) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            ExpandsMainDataModel = true;
            DeviceShaders = new Dictionary<ArtemisDevice, SKShader>();
            RainbowColors = new List<SKColor>();

            for (var i = 0; i < 9; i++)
            {
                if (i != 8)
                    RainbowColors.Add(SKColor.FromHsv(i * 32, 1, 1));
                else
                    RainbowColors.Add(SKColor.FromHsv(0, 1, 1));
            }

            surfaceService.SurfaceConfigurationUpdated += (sender, args) => DeviceShaders.Clear();
            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
        }

        public int MovePercentage { get; set; }

        public Dictionary<ArtemisDevice, SKShader> DeviceShaders { get; set; }
        public List<SKColor> RainbowColors { get; set; }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override void Update(double deltaTime)
        {
            MovePercentage++;
            if (MovePercentage > 100)
                MovePercentage = 0;

            base.Update(deltaTime);
        }


        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas)
        {
            foreach (var device in surface.Devices)
            {
                if (!DeviceShaders.ContainsKey(device))
                    DeviceShaders.Add(device,
                        SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(device.RenderRectangle.Width, device.RenderRectangle.Height), RainbowColors.ToArray(), SKShaderTileMode.Clamp));

                var brush = DeviceShaders[device];
                brush.TranslateTransform((int) Math.Round(device.RenderRectangle.Width / 100.0 * MovePercentage), 0);
                graphics.FillPath(brush, device.RenderPath);
                brush.TranslateTransform((int) Math.Round(device.RenderRectangle.Width / 100.0 * MovePercentage) * -1, 0);

                graphics.DrawRectangle(new Pen(Color.Red), device.RenderRectangle);
            }
        }

        public void RenderPerDevice(ArtemisSurface surface, Graphics graphics)
        {
            foreach (var device in surface.Devices)
            {
                if (!DeviceShaders.ContainsKey(device))
                    DeviceShaders.Add(device, new TextureBrush(RenderGradientForDevice(device), WrapMode.Tile));

                var brush = DeviceShaders[device];
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