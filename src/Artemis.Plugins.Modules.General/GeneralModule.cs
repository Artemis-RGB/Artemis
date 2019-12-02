using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Plugins.Modules.General.ViewModels;
using SkiaSharp;

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
                    RainbowColors.Add(SKColor.FromHsv(i * 32, 100, 100));
                else
                    RainbowColors.Add(SKColor.FromHsv(0, 100, 100));
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
            return;
            foreach (var device in surface.Devices)
            {
                using (var bitmap = new SKBitmap(new SKImageInfo((int) device.RenderRectangle.Width, (int) device.RenderRectangle.Height)))
                {
                    using (var layerCanvas = new SKCanvas(bitmap))
                    {
                        layerCanvas.Clear();

                        var shader = SKShader.CreateLinearGradient(
                            new SKPoint(0, 0),
                            new SKPoint(device.RenderRectangle.Width, 0),
                            RainbowColors.ToArray(),
                            null,
                            SKShaderTileMode.Clamp);

                        // use the shader
                        var paint = new SKPaint
                        {
                            Shader = shader
                        };

                        layerCanvas.DrawRect(0, 0, device.RenderRectangle.Width, device.RenderRectangle.Height, paint);
                    }

                    var bitmapShader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);

                    canvas.Save();
                    canvas.ClipRect(device.RenderRectangle);

                    var scaledRect = SKRect.Create(device.RenderRectangle.Left, device.RenderRectangle.Top, device.RenderRectangle.Width * 2, device.RenderRectangle.Height * 2);
                    canvas.Translate(device.RenderRectangle.Width / 100 * MovePercentage * -1, 0);
                    canvas.DrawRect(scaledRect, new SKPaint {Shader = bitmapShader});
                    canvas.Restore();
                }
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