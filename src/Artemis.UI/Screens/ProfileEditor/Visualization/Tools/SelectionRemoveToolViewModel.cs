using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Properties;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public class SelectionRemoveToolViewModel : VisualizationToolViewModel
    {
        private Rect _dragRectangle;

        public SelectionRemoveToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            using (MemoryStream stream = new MemoryStream(Resources.aero_pen_min))
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
            ProfileViewModel.SelectedLeds.Clear();
            ProfileViewModel.SelectedLeds.AddRange(selectedLeds);

            // Apply the selection to the selected layer layer
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                List<ArtemisLed> remainingLeds = layer.Leds.Except(selectedLeds).ToList();
                layer.ClearLeds();
                layer.AddLeds(remainingLeds);

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

            Point position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            Rect selectedRect = new Rect(MouseDownStartPosition, position);
            List<ArtemisLed> selectedLeds = ProfileViewModel.GetLedsInRectangle(selectedRect);

            // Unless shift is held down, clear the current selection
            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                ProfileViewModel.SelectedLeds.Clear();
            ProfileViewModel.SelectedLeds.AddRange(selectedLeds.Except(ProfileViewModel.SelectedLeds));

            DragRectangle = selectedRect;
        }
    }
}