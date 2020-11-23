using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Properties;
using Artemis.UI.Shared.Services;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public class SelectionToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerBrushService _layerBrushService;
        private Rect _dragRectangle;

        public SelectionToolViewModel(ProfileViewModel profileViewModel,
            IProfileEditorService profileEditorService,
            ILayerBrushService layerBrushService) : base(profileViewModel, profileEditorService)
        {
            _layerBrushService = layerBrushService;
            using (MemoryStream stream = new MemoryStream(Resources.aero_crosshair))
            {
                Cursor = new Cursor(stream);
            }
        }

        public Rect DragRectangle
        {
            get => _dragRectangle;
            set => SetAndNotify(ref _dragRectangle, value);
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            Point position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            Rect selectedRect = new Rect(MouseDownStartPosition, position);

            // Get selected LEDs
            List<ArtemisLed> selectedLeds = ProfileViewModel.GetLedsInRectangle(selectedRect);

            // Apply the selection to the selected layer layer
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                // If shift is held down, add to the selection instead of replacing it
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    layer.AddLeds(selectedLeds.Except(layer.Leds));
                else
                {
                    layer.ClearLeds();
                    layer.AddLeds(selectedLeds);
                }

                ProfileEditorService.UpdateSelectedProfileElement();
            }
            // If no layer selected, apply it to a new layer in the selected folder
            else if (ProfileEditorService.SelectedProfileElement is Folder folder)
                CreateLayer(folder, selectedLeds);
            // If no folder selected, apply it to a new layer in the root folder
            else
            {
                Folder rootFolder = ProfileEditorService.SelectedProfile.GetRootFolder();
                CreateLayer(rootFolder, selectedLeds);
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

            Point position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            Rect selectedRect = new Rect(MouseDownStartPosition, position);
            List<ArtemisLed> selectedLeds = ProfileViewModel.GetLedsInRectangle(selectedRect);

            // Unless shift is held down, clear the current selection
            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                ProfileViewModel.SelectedLeds.Clear();
            ProfileViewModel.SelectedLeds.AddRange(selectedLeds.Except(ProfileViewModel.SelectedLeds));

            DragRectangle = selectedRect;
        }

        private void CreateLayer(Folder folder, List<ArtemisLed> selectedLeds)
        {
            Layer newLayer = new Layer(folder, "New layer");

            LayerBrushDescriptor brush = _layerBrushService.GetDefaultLayerBrush();
            if (brush != null)
                newLayer.ChangeLayerBrush(brush);

            newLayer.AddLeds(selectedLeds);
            ProfileEditorService.ChangeSelectedProfileElement(newLayer);
            ProfileEditorService.UpdateSelectedProfileElement();
        }
    }
}