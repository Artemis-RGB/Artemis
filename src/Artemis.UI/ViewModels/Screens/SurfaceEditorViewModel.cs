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
                device.SetColorsEnabled(false);

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
                    device.SetColorsEnabled(false);
                    Devices.Add(device);
                }
            });
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            foreach (var rgbDeviceViewModel in Devices)
                rgbDeviceViewModel.Update();
        }

        #region Selection

        private bool _editorSelecting;
        private Point _selectionStartPoint;

        private void StartSelection(Point position)
        {
            _selectionStartPoint = position;
            _editorSelecting = true;

            SelectionRectangle.Rect = new Rect();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void StopSelection(Point position)
        {
            _editorSelecting = false;
            Mouse.OverrideCursor = null;

            var selectedRect = new Rect(_selectionStartPoint, position);
            foreach (var device in Devices)
            {
                if (device.DeviceRectangle.IntersectsWith(selectedRect))
                    device.SelectionStatus = SelectionStatus.Selected;
                else
                    device.SelectionStatus = SelectionStatus.None;
            }
        }

        private void UpdateSelection(Point position)
        {
            var selectedRect = new Rect(_selectionStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            foreach (var device in Devices)
            {
                if (device.DeviceRectangle.IntersectsWith(selectedRect))
                    device.SelectionStatus = SelectionStatus.Selected;
                else
                    device.SelectionStatus = SelectionStatus.None;
            }
        }

        public void EditorGridMouseClick(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition((IInputElement) sender);
            if (e.LeftButton == MouseButtonState.Pressed)
                StartSelection(position);
            else
                StopSelection(position);
        }

        public void EditorGridMouseMove(object sender, MouseEventArgs e)
        {
            if (!_editorSelecting)
                return;

            var position = e.GetPosition((IInputElement) sender);
            UpdateSelection(position);
        }

        #endregion
    }
}