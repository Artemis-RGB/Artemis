using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using Artemis.UI.ViewModels.Controls.SurfaceEditor;
using Artemis.UI.ViewModels.Interfaces;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.ViewModels.Screens
{
    public class SurfaceEditorViewModel : Screen, ISurfaceEditorViewModel
    {
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;

        public SurfaceEditorViewModel(IRgbService rgbService, ISurfaceService surfaceService)
        {
            Devices = new ObservableCollection<SurfaceDeviceViewModel>();
            SurfaceConfigurations = new ObservableCollection<SurfaceConfiguration>();
            SelectionRectangle = new RectangleGeometry();

            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;

            foreach (var surfaceDevice in _rgbService.LoadedDevices)
            {
                var device = new SurfaceDeviceViewModel(surfaceDevice) {Cursor = Cursors.Hand};
                Devices.Add(device);
                device.ZIndex = Devices.IndexOf(device) + 1;
            }
        }

        public RectangleGeometry SelectionRectangle { get; set; }
        public ObservableCollection<SurfaceDeviceViewModel> Devices { get; set; }

        public SurfaceConfiguration SelectedSurfaceConfiguration { get; set; }
        public ObservableCollection<SurfaceConfiguration> SurfaceConfigurations { get; set; }
        public string NewConfigurationName { get; set; }

        public string Title => "Surface Editor";

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            Execute.OnUIThread(() =>
            {
                if (Devices.All(d => d.Device != e.Device))
                {
                    var device = new SurfaceDeviceViewModel(e.Device) {Cursor = Cursors.Hand};
                    Devices.Add(device);
                    device.ZIndex = Devices.IndexOf(device) + 1;
                }
            });
        }

        private async Task LoadSurfaceConfigurations()
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                SurfaceConfigurations.Clear();

                // Get surface configs
                var configs = await _surfaceService.GetSurfaceConfigurationsAsync();
                // Populate the UI collection
                foreach (var surfaceConfiguration in configs)
                    SurfaceConfigurations.Add(surfaceConfiguration);

                // Select either the first active surface or the first available surface
                SelectedSurfaceConfiguration = SurfaceConfigurations.FirstOrDefault(s => s.IsActive) ?? SurfaceConfigurations.FirstOrDefault();

                // Create a default if there is none
                if (SelectedSurfaceConfiguration == null)
                    SelectedSurfaceConfiguration = AddSurfaceConfiguration("Default");
            });
        }

        public SurfaceConfiguration AddSurfaceConfiguration(string name)
        {
            var config = _surfaceService.CreateSurfaceConfiguration(name);
            Execute.OnUIThread(() => SurfaceConfigurations.Add(config));
            return config;
        }

        public void ConfirmationDialogClosing()
        {
            if (!string.IsNullOrWhiteSpace(NewConfigurationName))
            {
                var newConfig = AddSurfaceConfiguration(NewConfigurationName);
                SelectedSurfaceConfiguration = newConfig;
            }

            NewConfigurationName = null;
        }

        #region Context menu actions

        public void BringToFront(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            Devices.Move(Devices.IndexOf(surfaceDeviceViewModel), Devices.Count - 1);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.ZIndex = i + 1;
            }
        }

        public void BringForward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            var currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            var newIndex = Math.Min(currentIndex + 1, Devices.Count - 1);
            Devices.Move(currentIndex, newIndex);

            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.ZIndex = i + 1;
            }
        }

        public void SendToBack(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            Devices.Move(Devices.IndexOf(surfaceDeviceViewModel), 0);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.ZIndex = i + 1;
            }
        }

        public void SendBackward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            var currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            var newIndex = Math.Max(currentIndex - 1, 0);
            Devices.Move(currentIndex, newIndex);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.ZIndex = i + 1;
            }
        }

        #endregion

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

        #region Overrides of Screen

        protected override void OnActivate()
        {
            Task.Run(LoadSurfaceConfigurations);
            base.OnActivate();
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