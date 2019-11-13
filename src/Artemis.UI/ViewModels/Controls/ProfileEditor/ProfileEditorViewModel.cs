using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage;
using Artemis.UI.ViewModels.Screens;
using Artemis.UI.ViewModels.Utilities;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
    public class ProfileEditorViewModel : ModuleViewModel
    {
        private readonly TimerUpdateTrigger _updateTrigger;

        public ProfileEditorViewModel(Module module, ISurfaceService surfaceService, ISettingsService settingsService) : base(module, "Profile Editor")
        {
            surfaceService.ActiveSurfaceConfigurationChanged += OnActiveSurfaceConfigurationChanged;
            Devices = new ObservableCollection<ProfileDeviceViewModel>();
            Execute.OnUIThread(() =>
            {
                SelectionRectangle = new RectangleGeometry();
                PanZoomViewModel = new PanZoomViewModel();
            });

            ApplySurfaceConfiguration(surfaceService.ActiveSurface);

            // Borrow RGB.NET's update trigger, update up to 25 FPS, ignore higher settings than that
            var targetFps = Math.Min(settingsService.GetSetting("TargetFrameRate", 25).Value, 25);
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / targetFps};
            _updateTrigger.Update += UpdateLeds;
        }

        public ObservableCollection<ProfileDeviceViewModel> Devices { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }
        public PanZoomViewModel PanZoomViewModel { get; set; }

        private void OnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
        }

        private void UpdateLeds(object sender, CustomUpdateData customUpdateData)
        {
            foreach (var profileDeviceViewModel in Devices)
                profileDeviceViewModel.Update();
        }

        private void ApplySurfaceConfiguration(Surface surface)
        {
            // Make sure all devices have an up-to-date VM
            foreach (var surfaceDeviceConfiguration in surface.Devices)
            {
                // Create VMs for missing devices
                var viewModel = Devices.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
                if (viewModel == null)
                    Execute.OnUIThread(() => Devices.Add(new ProfileDeviceViewModel(surfaceDeviceConfiguration)));
                // Update existing devices
                else
                    viewModel.Device = surfaceDeviceConfiguration;
            }

            // Sort the devices by ZIndex
            Execute.OnUIThread(() =>
            {
                foreach (var device in Devices.OrderBy(d => d.ZIndex).ToList()) 
                    Devices.Move(Devices.IndexOf(device), device.ZIndex - 1);
            });
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
        private Point _mouseDragStartPoint;

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
            if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(position);
        }

        private void StartMouseDrag(Point position, Point relative)
        {
            _mouseDragStatus = MouseDragStatus.Selecting;
            _mouseDragStartPoint = position;

            // Any time dragging starts, start with a new rect
            SelectionRectangle.Rect = new Rect();
        }

        private void StopMouseDrag(Point position)
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