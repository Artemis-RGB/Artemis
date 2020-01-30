using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using Artemis.UI.Properties;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class SelectionRemoveToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;

        public SelectionRemoveToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService) : base(profileViewModel,
            profileEditorService)
        {
            _layerEditorService = layerEditorService;
            using (var stream = new MemoryStream(Resources.aero_pen_min))
            {
                Cursor = new Cursor(stream);
            }
        }

        public Rect DragRectangle { get; set; }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            var position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            var selectedRect = new Rect(MouseDownStartPosition, position);

            // Get selected LEDs
            var selectedLeds = new List<ArtemisLed>();
            foreach (var device in ProfileViewModel.DeviceViewModels)
            {
                foreach (var ledViewModel in device.Leds)
                {
                    if (ledViewModel.Led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1).IntersectsWith(selectedRect))
                        selectedLeds.Add(ledViewModel.Led);
                    // Unselect everything
                    ledViewModel.IsSelected = false;
                }
            }

            // Apply the selection to the selected layer layer
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                // If the layer has a shape, save it's size
                var shapeSize = SKRect.Empty;
                if (layer.LayerShape != null)
                    shapeSize = layer.LayerShape.GetUnscaledRectangle();

                var remainingLeds = layer.Leds.Except(selectedLeds).ToList();
                layer.ClearLeds();
                layer.AddLeds(remainingLeds);
                
                // Restore the saved size
                if (layer.LayerShape != null)
                    layer.LayerShape.SetFromUnscaledRectangle(shapeSize);
                
                ProfileEditorService.UpdateSelectedProfileElement();
            }
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            if (!IsMouseDown)
            {
                DragRectangle = new Rect(-1, -1, 0, 0);
                return;
            }

            var position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            var selectedRect = new Rect(MouseDownStartPosition, position);

            foreach (var device in ProfileViewModel.DeviceViewModels)
            {
                foreach (var ledViewModel in device.Leds)
                {
                    if (ledViewModel.Led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1).IntersectsWith(selectedRect))
                        ledViewModel.IsSelected = true;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        ledViewModel.IsSelected = false;
                }
            }

            DragRectangle = selectedRect;
        }
    }
}