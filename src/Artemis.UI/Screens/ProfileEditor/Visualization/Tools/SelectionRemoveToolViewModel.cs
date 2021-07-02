using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Properties;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public class SelectionRemoveToolViewModel : VisualizationToolViewModel
    {
        private Rect _dragRectangle;

        public SelectionRemoveToolViewModel(PanZoomViewModel panZoomViewModel, IProfileEditorService profileEditorService) : base(panZoomViewModel, profileEditorService)
        {
            using (MemoryStream stream = new(Resources.aero_pen_min))
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

            if (ProfileEditorService.SuspendEditing)
                return;

            Point position = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            Rect selectedRect = new(MouseDownStartPosition, position);

            // Get selected LEDs
            List<ArtemisLed> selectedLeds = ProfileEditorService.GetLedsInRectangle(selectedRect);

            // Apply the selection to the selected layer layer
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                List<ArtemisLed> remainingLeds = layer.Leds.Except(selectedLeds).ToList();
                layer.ClearLeds();
                layer.AddLeds(remainingLeds);

                ProfileEditorService.SaveSelectedProfileElement();
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

            Point position = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            Rect selectedRect = new(MouseDownStartPosition, position);
            DragRectangle = selectedRect;
        }
    }
}