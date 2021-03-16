using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Screens.GradientEditor;
using SkiaSharp;

namespace Artemis.UI.Shared.Services
{
    internal class ColorPickerService : IColorPickerService
    {
        private readonly ICoreService _coreService;
        private readonly IDialogService _dialogService;

        private bool _mustRenderOverlay;
        private SKColor _overlayColor;
        private float _overlayOpacity;

        public ColorPickerService(IDialogService dialogService, ICoreService coreService, ISettingsService settingsService)
        {
            _dialogService = dialogService;
            _coreService = coreService;

            PreviewSetting = settingsService.GetSetting("UI.PreviewColorPickerOnDevices", false);
            PreviewSetting.AutoSave = true;
            RecentColorsSetting = settingsService.GetSetting("UI.ColorPickerRecentColors", new LinkedList<Color>());
        }

        private void RenderColorPickerOverlay(object? sender, FrameRenderingEventArgs e)
        {
            if (_mustRenderOverlay)
                _overlayOpacity += 0.2f;
            else
                _overlayOpacity -= 0.2f;

            if (_overlayOpacity <= 0f)
            {
                _coreService.FrameRendering -= RenderColorPickerOverlay;
                return;
            }

            if (_overlayOpacity > 1f)
                _overlayOpacity = 1f;

            using SKPaint overlayPaint = new() {Color = new SKColor(0, 0, 0, (byte) (255 * _overlayOpacity))};
            overlayPaint.Color = _overlayColor.WithAlpha((byte) (_overlayColor.Alpha * _overlayOpacity));
            e.Canvas.DrawRect(0, 0, e.Canvas.LocalClipBounds.Width, e.Canvas.LocalClipBounds.Height, overlayPaint);
        }

        public PluginSetting<bool> PreviewSetting { get; }
        public PluginSetting<LinkedList<Color>> RecentColorsSetting { get; }

        public LinkedList<Color> RecentColors => RecentColorsSetting.Value;

        public Task<object> ShowGradientPicker(ColorGradient colorGradient, string dialogHost)
        {
            if (!string.IsNullOrWhiteSpace(dialogHost))
                return _dialogService.ShowDialogAt<GradientEditorViewModel>(dialogHost, new Dictionary<string, object> {{"colorGradient", colorGradient}});
            return _dialogService.ShowDialog<GradientEditorViewModel>(new Dictionary<string, object> {{"colorGradient", colorGradient}});
        }

        public void StartColorDisplay()
        {
            if (_mustRenderOverlay)
                return;

            _overlayOpacity = 0f;
            _mustRenderOverlay = true;

            _coreService.FrameRendering += RenderColorPickerOverlay;
        }

        public void StopColorDisplay()
        {
            _mustRenderOverlay = false;
        }

        public void UpdateColorDisplay(Color color)
        {
            _overlayColor = new SKColor(color.R, color.G, color.B, color.A);
        }

        public void QueueRecentColor(Color color)
        {
            if (RecentColors.Contains(color)) 
                RecentColors.Remove(color);

            RecentColors.AddFirst(color);
            while (RecentColors.Count > 18)
                RecentColors.RemoveLast();

            RecentColorsSetting.Save();
        }
    }
}