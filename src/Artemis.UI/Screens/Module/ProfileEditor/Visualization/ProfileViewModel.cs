using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileViewModel : ProfileEditorPanelViewModel
    {
        private readonly TimerUpdateTrigger _updateTrigger;

        public ProfileViewModel(ISurfaceService surfaceService, ISettingsService settingsService)
        {
            Devices = new ObservableCollection<ProfileDeviceViewModel>();
            Execute.OnUIThread(() =>
            {
                SelectionRectangle = new RectangleGeometry();
                PanZoomViewModel = new PanZoomViewModel();
            });

            ApplySurfaceConfiguration(surfaceService.ActiveSurface);

            // Borrow RGB.NET's update trigger but limit the FPS
            var targetFpsSetting = settingsService.GetSetting("TargetFrameRate", 25);
            var editorTargetFpsSetting = settingsService.GetSetting("EditorTargetFrameRate", 15);
            var targetFps = Math.Min(targetFpsSetting.Value, editorTargetFpsSetting.Value);
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / targetFps};
            _updateTrigger.Update += UpdateLeds;

            surfaceService.ActiveSurfaceConfigurationChanged += OnActiveSurfaceConfigurationChanged;
        }

        public bool IsInitializing { get; private set; }
        public ObservableCollection<ProfileDeviceViewModel> Devices { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }
        public PanZoomViewModel PanZoomViewModel { get; set; }

        private void OnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
        }

        private void ApplySurfaceConfiguration(Surface surface)
        {
            // Make sure all devices have an up-to-date VM
            foreach (var surfaceDeviceConfiguration in surface.Devices)
            {
                // Create VMs for missing devices
                var viewModel = Devices.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
                if (viewModel == null)
                {
                    // Create outside the UI thread to avoid slowdowns as much as possible
                    var profileDeviceViewModel = new ProfileDeviceViewModel(surfaceDeviceConfiguration);
                    Execute.OnUIThread(() =>
                    {
                        // Gotta call IsInitializing on the UI thread or its never gets picked up
                        IsInitializing = true;
                        lock (Devices)
                        {
                            Devices.Add(profileDeviceViewModel);
                        }
                    });
                }
                // Update existing devices
                else
                    viewModel.Device = surfaceDeviceConfiguration;
            }

            // Sort the devices by ZIndex
            Execute.OnUIThread(() =>
            {
                lock (Devices)
                {
                    foreach (var device in Devices.OrderBy(d => d.ZIndex).ToList())
                        Devices.Move(Devices.IndexOf(device), device.ZIndex - 1);
                }
            });
        }

        private void UpdateLeds(object sender, CustomUpdateData customUpdateData)
        {
            if (IsInitializing)
                IsInitializing = Devices.Any(d => !d.AddedLeds);

            foreach (var profileDeviceViewModel in Devices)
                profileDeviceViewModel.Update();
        }

        protected override void OnActivate()
        {
            _updateTrigger.Start();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _updateTrigger.Stop();
            base.OnDeactivate();
        }

        #region Selection

        private MouseDragStatus _mouseDragStatus;
        private System.Windows.Point _mouseDragStartPoint;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseEventArgs e)
        {
            if (IsPanKeyDown())
                return;

            var position = e.GetPosition((IInputElement)sender);
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

            var position = e.GetPosition((IInputElement)sender);
            if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(position);
        }

        private void StartMouseDrag(System.Windows.Point position, System.Windows.Point relative)
        {
            _mouseDragStatus = MouseDragStatus.Selecting;
            _mouseDragStartPoint = position;

            // Any time dragging starts, start with a new rect
            SelectionRectangle.Rect = new Rect();
        }

        private void StopMouseDrag(System.Windows.Point position)
        {
            var selectedRect = new Rect(_mouseDragStartPoint, position);
            // TODO: Select LEDs

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

                // TODO: Highlight LEDs
            }
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
}