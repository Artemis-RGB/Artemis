using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using Artemis.UI.ViewModels.Controls.SurfaceEditor;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

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
            PanZoomViewModel = new PanZoomViewModel();

            _rgbService = rgbService;
            _surfaceService = surfaceService;
        }

        public RectangleGeometry SelectionRectangle { get; set; }
        public ObservableCollection<SurfaceDeviceViewModel> Devices { get; set; }

        public SurfaceConfiguration SelectedSurfaceConfiguration
        {
            get => _selectedSurfaceConfiguration;
            set
            {
                _selectedSurfaceConfiguration = value;
                ApplySelectedSurfaceConfiguration();
            }
        }

        public ObservableCollection<SurfaceConfiguration> SurfaceConfigurations { get; set; }
        public string NewConfigurationName { get; set; }

        public PanZoomViewModel PanZoomViewModel { get; set; }

        public string Title => "Surface Editor";

        public SurfaceConfiguration AddSurfaceConfiguration(string name)
        {
            var config = _surfaceService.CreateSurfaceConfiguration(name);
            Execute.OnUIThread(() => SurfaceConfigurations.Add(config));
            return config;
        }

        private void LoadSurfaceConfigurations()
        {
            // Get surface configs
            var configs = _surfaceService.SurfaceConfigurations;

            // Get the active config, if empty, create a default config
            var activeConfig = _surfaceService.ActiveSurfaceConfiguration;
            if (activeConfig == null)
            {
                activeConfig = AddSurfaceConfiguration("Default");
                _surfaceService.ActiveSurfaceConfiguration = activeConfig;
            }

            Execute.OnUIThread(() =>
            {
                // Populate the UI collection
                SurfaceConfigurations.Clear();
                foreach (var surfaceConfiguration in configs)
                    SurfaceConfigurations.Add(surfaceConfiguration);

                // Set the active config
                SelectedSurfaceConfiguration = activeConfig;
            });
        }

        private void ApplySelectedSurfaceConfiguration()
        {
            if (SelectedSurfaceConfiguration == null)
            {
                Execute.OnUIThread(Devices.Clear);
                return;
            }

            Task.Run(() =>
            {
                // Create VMs for the new config outside the UI thread
                var viewModels = SelectedSurfaceConfiguration.DeviceConfigurations.Select(c => new SurfaceDeviceViewModel(c)).ToList();
                // Commit the VMs to the view
                Execute.OnUIThread(() =>
                {
                    Devices.Clear();
                    foreach (var viewModel in viewModels.OrderBy(v => v.ZIndex))
                        Devices.Add(viewModel);
                });
            });
        }

        #region Overrides of Screen

        protected override void OnActivate()
        {
            LoadSurfaceConfigurations();
            base.OnActivate();
        }

        #endregion

        #region Configuration management

        public void ConfirmationDialogClosing()
        {
            if (!string.IsNullOrWhiteSpace(NewConfigurationName))
            {
                var newConfig = AddSurfaceConfiguration(NewConfigurationName);
                SelectedSurfaceConfiguration = newConfig;
            }

            NewConfigurationName = null;
        }

        #endregion

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

        #region Device selection

        private MouseDragStatus _mouseDragStatus;
        private Point _mouseDragStartPoint;
        private SurfaceConfiguration _selectedSurfaceConfiguration;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseEventArgs e)
        {
            if (IsPanKeyDown())
                return;

            var position = e.GetPosition((IInputElement) sender);
            var relative = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (e.LeftButton == MouseButtonState.Pressed)
                StartMouseDrag(position, relative);
            else
                StopMouseDrag(position);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseMove(object sender, MouseEventArgs e)
        {
            // If holding down Ctrl, pan instead of move/select
            if (IsPanKeyDown())
            {
                Pan(sender, e);
                return;
            }

            var position = e.GetPosition((IInputElement) sender);
            var relative = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (_mouseDragStatus == MouseDragStatus.Dragging)
                MoveSelected(relative);
            else if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(position);
        }

        private void StartMouseDrag(Point position, Point relative)
        {
            // If drag started on top of a device, initialise dragging
            var device = Devices.LastOrDefault(d => PanZoomViewModel.TransformContainingRect(d.DeviceRectangle).Contains(position));
            if (device != null)
            {
                _mouseDragStatus = MouseDragStatus.Dragging;
                // If the device is not selected, deselect others and select only this one (if shift not held)
                if (device.SelectionStatus != SelectionStatus.Selected)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    {
                        foreach (var others in Devices)
                            others.SelectionStatus = SelectionStatus.None;
                    }

                    device.SelectionStatus = SelectionStatus.Selected;
                }

                foreach (var selectedDevice in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
                    selectedDevice.StartMouseDrag(relative);
            }
            // Start multi-selection
            else
            {
                _mouseDragStatus = MouseDragStatus.Selecting;
                _mouseDragStartPoint = position;
            }

            // While dragging always show a hand to avoid cursor flicker
            if (_mouseDragStatus == MouseDragStatus.Dragging)
                Mouse.OverrideCursor = Cursors.Hand;
            else
                Mouse.OverrideCursor = Cursors.Arrow;

            // Any time dragging starts, start with a new rect
            SelectionRectangle.Rect = new Rect();
        }

        private void StopMouseDrag(Point position)
        {
            if (_mouseDragStatus != MouseDragStatus.Dragging)
            {
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                foreach (var device in Devices)
                {
                    if (PanZoomViewModel.TransformContainingRect(device.DeviceRectangle).IntersectsWith(selectedRect))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        device.SelectionStatus = SelectionStatus.None;
                }
            }
            else
            {
                foreach (var device in Devices)
                    device.DeviceConfiguration.ApplyToDevice();
                _surfaceService.SaveToRepository(SelectedSurfaceConfiguration, true);
            }

            Mouse.OverrideCursor = null;
            _mouseDragStatus = MouseDragStatus.None;
        }

        private void UpdateSelection(Point position)
        {
            if (IsPanKeyDown())
                return;

            lock (Devices)
            {
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;

                foreach (var device in Devices)
                {
                    if (PanZoomViewModel.TransformContainingRect(device.DeviceRectangle).IntersectsWith(selectedRect))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        device.SelectionStatus = SelectionStatus.None;
                }
            }
        }

        private void MoveSelected(Point position)
        {
            foreach (var device in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
                device.UpdateMouseDrag(position);
        }

        #endregion

        #region Panning and zooming

        public void EditorGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            PanZoomViewModel.ProcessMouseScroll(sender, e);
        }

        public void EditorGridKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsDown)
                Mouse.OverrideCursor = Cursors.ScrollAll;
        }

        public void EditorGridKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsUp)
                Mouse.OverrideCursor = null;
        }

        public void Pan(object sender, MouseEventArgs e)
        {
            PanZoomViewModel.ProcessMouseMove(sender, e);

            // Empty the selection rect since it's shown while mouse is down
            SelectionRectangle.Rect = Rect.Empty;
        }

        public void ResetZoomAndPan()
        {
            PanZoomViewModel.Reset();
        }

        private bool IsPanKeyDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
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