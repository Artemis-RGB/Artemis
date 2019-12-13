using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Services.Interfaces;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileViewModel : ProfileEditorPanelViewModel, IHandle<MainWindowFocusChangedEvent>
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private TimerUpdateTrigger _updateTrigger;

        public ProfileViewModel(IProfileEditorService profileEditorService,
            ISurfaceService surfaceService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator)
        {
            _profileEditorService = profileEditorService;
            _surfaceService = surfaceService;
            _settingsService = settingsService;
            Devices = new ObservableCollection<ProfileDeviceViewModel>();
            Cursor = null;

            Execute.PostToUIThread(() =>
            {
                SelectionRectangle = new RectangleGeometry();
                PanZoomViewModel = new PanZoomViewModel();
            });

            ApplySurfaceConfiguration(surfaceService.ActiveSurface);
            CreateUpdateTrigger();

            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementChanged;
            eventAggregator.Subscribe(this);
        }

        public bool IsInitializing { get; private set; }
        public ObservableCollection<ProfileDeviceViewModel> Devices { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }
        public PanZoomViewModel PanZoomViewModel { get; set; }
        public PluginSetting<bool> HighlightSelectedLayer { get; set; }
        public PluginSetting<bool> PauseRenderingOnFocusLoss { get; set; }

        public Cursor Cursor { get; set; }

        private void CreateUpdateTrigger()
        {
            // Borrow RGB.NET's update trigger but limit the FPS
            var targetFpsSetting = _settingsService.GetSetting("Core.TargetFrameRate", 25);
            var editorTargetFpsSetting = _settingsService.GetSetting("ProfileEditor.TargetFrameRate", 15);
            var targetFps = Math.Min(targetFpsSetting.Value, editorTargetFpsSetting.Value);
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / targetFps};
            _updateTrigger.Update += UpdateLeds;

            _surfaceService.ActiveSurfaceConfigurationChanged += OnActiveSurfaceConfigurationChanged;
        }

        private void OnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
        }

        private void ApplySurfaceConfiguration(ArtemisSurface surface)
        {
            List<ArtemisDevice> devices;
            lock (Devices)
            {
                devices = new List<ArtemisDevice>();
                devices.AddRange(surface.Devices);
            }

            // Make sure all devices have an up-to-date VM
            foreach (var surfaceDeviceConfiguration in devices)
            {
                // Create VMs for missing devices
                var viewModel = Devices.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
                if (viewModel == null)
                {
                    // Create outside the UI thread to avoid slowdowns as much as possible
                    var profileDeviceViewModel = new ProfileDeviceViewModel(surfaceDeviceConfiguration);
                    Execute.PostToUIThread(() =>
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
            Execute.PostToUIThread(() =>
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
            lock (Devices)
            {
                if (IsInitializing)
                    IsInitializing = Devices.Any(d => !d.AddedLeds);

                foreach (var profileDeviceViewModel in Devices)
                    profileDeviceViewModel.Update();
            }
        }

        private void UpdateLedsDimStatus()
        {
            if (HighlightSelectedLayer.Value && _profileEditorService.SelectedProfileElement is Layer layer)
            {
                foreach (var led in Devices.SelectMany(d => d.Leds))
                    led.IsDimmed = !layer.Leds.Contains(led.Led);
            }
            else
            {
                foreach (var led in Devices.SelectMany(d => d.Leds))
                    led.IsDimmed = false;
            }
        }

        protected override void OnActivate()
        {
            HighlightSelectedLayer = _settingsService.GetSetting("ProfileEditor.HighlightSelectedLayer", true);
            PauseRenderingOnFocusLoss = _settingsService.GetSetting("ProfileEditor.PauseRenderingOnFocusLoss", true);

            HighlightSelectedLayer.SettingChanged += HighlightSelectedLayerOnSettingChanged;

            _updateTrigger.Start();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            HighlightSelectedLayer.Save();
            PauseRenderingOnFocusLoss.Save();
            
            _updateTrigger.Stop();
            base.OnDeactivate();
        }

        #region Context menu actions

        public bool CanApplyToLayer { get; set; }

        public void CreateLayer()
        {
        }

        public void ApplyToLayer()
        {
            if (!(_profileEditorService.SelectedProfileElement is Layer layer))
                return;

            layer.ClearLeds();
            layer.AddLeds(Devices.SelectMany(d => d.Leds).Where(vm => vm.IsSelected).Select(vm => vm.Led));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void SelectAll()
        {
            foreach (var ledVm in Devices.SelectMany(d => d.Leds))
                ledVm.IsSelected = true;
        }

        public void InverseSelection()
        {
            foreach (var ledVm in Devices.SelectMany(d => d.Leds))
                ledVm.IsSelected = !ledVm.IsSelected;
        }

        public void ClearSelection()
        {
            foreach (var ledVm in Devices.SelectMany(d => d.Leds))
                ledVm.IsSelected = false;
        }

        #endregion

        #region Selection

        private MouseDragStatus _mouseDragStatus;
        private Point _mouseDragStartPoint;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (IsPanKeyDown() || e.ChangedButton == MouseButton.Right)
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
            foreach (var device in Devices)
            {
                foreach (var profileLedViewModel in device.Leds)
                {
                    if (PanZoomViewModel.TransformContainingRect(profileLedViewModel.Led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1)).IntersectsWith(selectedRect))
                        profileLedViewModel.IsSelected = true;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        profileLedViewModel.IsSelected = false;
                }
            }

            _mouseDragStatus = MouseDragStatus.None;
        }

        private void UpdateSelection(Point position)
        {
            if (IsPanKeyDown())
                return;

            var selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            foreach (var device in Devices)
            {
                foreach (var profileLedViewModel in device.Leds)
                {
                    if (PanZoomViewModel.TransformContainingRect(profileLedViewModel.Led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1)).IntersectsWith(selectedRect))
                        profileLedViewModel.IsSelected = true;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        profileLedViewModel.IsSelected = false;
                }
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
                Cursor = Cursors.ScrollAll;
        }

        public void EditorGridKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsUp)
                Cursor = null;
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

        #region Event handlers

        private void HighlightSelectedLayerOnSettingChanged(object sender, EventArgs e)
        {
            UpdateLedsDimStatus();
        }

        private void OnSelectedProfileElementChanged(object sender, EventArgs e)
        {
            UpdateLedsDimStatus();
            CanApplyToLayer = _profileEditorService.SelectedProfileElement is Layer;
        }

        public void Handle(MainWindowFocusChangedEvent message)
        {
            if (PauseRenderingOnFocusLoss == null || ScreenState != ScreenState.Active)
                return;

            if (PauseRenderingOnFocusLoss.Value && !message.IsFocused)
                _updateTrigger.Stop();
            else if (PauseRenderingOnFocusLoss.Value && message.IsFocused)
                _updateTrigger.Start();
        }

        #endregion
    }
}