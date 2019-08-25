using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Controls.RgbDevice;
using Artemis.UI.ViewModels.Interfaces;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.ViewModels.Screens
{
    public class SurfaceEditorViewModel : Screen, ISurfaceEditorViewModel
    {
        private readonly IRgbService _rgbService;


        public SurfaceEditorViewModel(IRgbService rgbService)
        {
            Devices = new ObservableCollection<RgbDeviceViewModel>();
            SelectionRectangle = new RectangleGeometry();

            _rgbService = rgbService;
            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
            _rgbService.Surface.Updated += SurfaceOnUpdated;

            foreach (var surfaceDevice in _rgbService.Surface.Devices)
            {
                var device = new RgbDeviceViewModel(surfaceDevice) {Cursor = Cursors.Hand};
//                device.SetColorsEnabled(false);

                Devices.Add(device);
            }
        }

        public RectangleGeometry SelectionRectangle { get; set; }
        public ObservableCollection<RgbDeviceViewModel> Devices { get; set; }

        public string Title => "Surface Editor";

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            Execute.OnUIThread(() =>
            {
                if (Devices.All(d => d.Device != e.Device))
                {
                    var device = new RgbDeviceViewModel(e.Device) {Cursor = Cursors.Hand};
//                    device.SetColorsEnabled(false);
                    Devices.Add(device);
                }
            });
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            foreach (var rgbDeviceViewModel in Devices)
                rgbDeviceViewModel.Update();
        }

        #region Mouse actions

        private MouseDragStatus _mouseDragStatus;
        private Point _mouseDragStartPoint;

        private void StartMouseDrag(Point position)
        {
            // If drag started on top of a device, initialise dragging
            var device = Devices.LastOrDefault(d => d.DeviceRectangle.Contains(position));
            if (device != null)
            {
                _mouseDragStatus = MouseDragStatus.Dragging;
                // If the device is not selected, deselect others and select only this one
                if (device.SelectionStatus != SelectionStatus.Selected)
                {
                    foreach (var others in Devices)
                        others.SelectionStatus = SelectionStatus.None;
                    device.SelectionStatus = SelectionStatus.Selected;
                }
                foreach (var selectedDevice in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
                    selectedDevice.StartMouseDrag(position);
            }
            // Start multi-selection
            else
            {
                _mouseDragStatus = MouseDragStatus.Selecting;
                _mouseDragStartPoint = position;
            }

            // While dragging always show an arrow to avoid cursor flicker
            Mouse.OverrideCursor = Cursors.Arrow;
            // Any time dragging starts, start with a new rect
            SelectionRectangle.Rect = new Rect();
        }

        private void StopMouseDrag(Point position)
        {
            if (_mouseDragStatus == MouseDragStatus.Dragging)
            {
            }
            else
            {
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                foreach (var device in Devices)
                {
                    if (device.DeviceRectangle.IntersectsWith(selectedRect))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else
                        device.SelectionStatus = SelectionStatus.None;
                }
            }

            Mouse.OverrideCursor = null;
            _mouseDragStatus = MouseDragStatus.None;
        }

        private void UpdateSelection(Point position)
        {
            lock (Devices)
            {
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;

                foreach (var device in Devices)
                {
                    if (device.DeviceRectangle.IntersectsWith(selectedRect))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else
                        device.SelectionStatus = SelectionStatus.None;
                }
            }
        }

        private void MoveSelected(Point position)
        {
            foreach (var device in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
                device.UpdateMouseDrag(position);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((IInputElement) sender);
            if (e.LeftButton == MouseButtonState.Pressed)
                StartMouseDrag(position);
            else
                StopMouseDrag(position);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((IInputElement) sender);
            if (_mouseDragStatus == MouseDragStatus.Dragging)
                MoveSelected(position);
            else if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(position);
        }

        #endregion
    }

    internal enum MouseDragStatus
    {
        None,
        Selecting,
        Dragging
    }
}