using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor.Dialogs;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceEditorViewModel : Screen, IMainScreenViewModel
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private ArtemisSurface _selectedSurface;
        private ObservableCollection<SurfaceDeviceViewModel> _devices;
        private ObservableCollection<ArtemisSurface> _surfaceConfigurations;
        private RectangleGeometry _selectionRectangle;
        private PanZoomViewModel _panZoomViewModel;
        private PluginSetting<GridLength> _surfaceListWidth;
        private Cursor _cursor;

        public SurfaceEditorViewModel(IRgbService rgbService, ISurfaceService surfaceService, IDialogService dialogService, ISettingsService settingsService,
            IDeviceService deviceService)
        {
            DisplayName = "Surface Editor";

            Devices = new ObservableCollection<SurfaceDeviceViewModel>();
            SurfaceConfigurations = new ObservableCollection<ArtemisSurface>();
            SelectionRectangle = new RectangleGeometry();
            PanZoomViewModel = new PanZoomViewModel();
            Cursor = null;

            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _deviceService = deviceService;
        }

        public ObservableCollection<SurfaceDeviceViewModel> Devices
        {
            get => _devices;
            set => SetAndNotify(ref _devices, value);
        }

        public ObservableCollection<ArtemisSurface> SurfaceConfigurations
        {
            get => _surfaceConfigurations;
            set => SetAndNotify(ref _surfaceConfigurations, value);
        }

        public RectangleGeometry SelectionRectangle
        {
            get => _selectionRectangle;
            set => SetAndNotify(ref _selectionRectangle, value);
        }

        public PanZoomViewModel PanZoomViewModel
        {
            get => _panZoomViewModel;
            set => SetAndNotify(ref _panZoomViewModel, value);
        }

        public PluginSetting<GridLength> SurfaceListWidth
        {
            get => _surfaceListWidth;
            set => SetAndNotify(ref _surfaceListWidth, value);
        }

        public Cursor Cursor
        {
            get => _cursor;
            set => SetAndNotify(ref _cursor, value);
        }

        public ArtemisSurface SelectedSurface
        {
            get => _selectedSurface;
            set
            {
                if (value == null)
                    return;

                SetAndNotify(ref _selectedSurface, value);
                ApplySelectedSurfaceConfiguration();
            }
        }

        public ArtemisSurface CreateSurfaceConfiguration(string name)
        {
            var config = _surfaceService.CreateSurfaceConfiguration(name);
            Execute.PostToUIThread(() => SurfaceConfigurations.Add(config));
            return config;
        }

        private void LoadWorkspaceSettings()
        {
            SurfaceListWidth = _settingsService.GetSetting("SurfaceEditor.SurfaceListWidth", new GridLength(300.0));
        }

        private void SaveWorkspaceSettings()
        {
            SurfaceListWidth.Save();
        }

        private void LoadSurfaceConfigurations()
        {
            // Get surface configs
            var configs = _surfaceService.SurfaceConfigurations.ToList();

            // Get the active config, if empty, create a default config
            var activeConfig = _surfaceService.ActiveSurface;
            if (activeConfig == null)
            {
                activeConfig = CreateSurfaceConfiguration("Default");
                configs.Add(activeConfig);
                _surfaceService.SetActiveSurfaceConfiguration(activeConfig);
            }

            Execute.PostToUIThread(() =>
            {
                // Populate the UI collection
                SurfaceConfigurations.Clear();
                foreach (var surfaceConfiguration in configs)
                    SurfaceConfigurations.Add(surfaceConfiguration);

                // Set the active config
                SelectedSurface = activeConfig;
            });
        }

        private void ApplySelectedSurfaceConfiguration()
        {
            // Make sure all devices have an up-to-date VM
            Execute.PostToUIThread(() =>
            {
                lock (Devices)
                {
                    var existing = Devices.ToList();
                    var deviceViewModels = new List<SurfaceDeviceViewModel>();

                    // Add missing/update existing
                    foreach (var surfaceDeviceConfiguration in SelectedSurface.Devices.OrderBy(d => d.ZIndex).ToList())
                    {
                        // Create VMs for missing devices
                        var viewModel = existing.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
                        if (viewModel == null)
                            viewModel = new SurfaceDeviceViewModel(surfaceDeviceConfiguration);
                        // Update existing devices
                        else
                            viewModel.Device = surfaceDeviceConfiguration;

                        // Add the viewModel to the list of VMs we want to keep
                        deviceViewModels.Add(viewModel);
                    }

                    Devices = new ObservableCollection<SurfaceDeviceViewModel>(deviceViewModels);
                }
            });

            _surfaceService.SetActiveSurfaceConfiguration(SelectedSurface);
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            LoadSurfaceConfigurations();
            LoadWorkspaceSettings();
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            SaveWorkspaceSettings();
            base.OnClose();
        }

        #endregion

        #region Configuration management

        public async Task DeleteSurfaceConfiguration(ArtemisSurface surface)
        {
            var result = await _dialogService.ShowConfirmDialogAt(
                "SurfaceListDialogHost",
                "Delete surface configuration",
                "Are you sure you want to delete\nthis surface configuration?"
            );
            if (result)
            {
                SurfaceConfigurations.Remove(surface);
                _surfaceService.DeleteSurfaceConfiguration(surface);
            }
        }

        public async Task AddSurfaceConfiguration()
        {
            var result = await _dialogService.ShowDialogAt<SurfaceCreateViewModel>("SurfaceListDialogHost");
            if (result is string name)
                CreateSurfaceConfiguration(name);
        }

        #endregion

        #region Context menu actions

        public void IdentifyDevice(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            _deviceService.IdentifyDevice(surfaceDeviceViewModel.Device);
        }

        public void BringToFront(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            Devices.Move(Devices.IndexOf(surfaceDeviceViewModel), Devices.Count - 1);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void BringForward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            var currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            var newIndex = Math.Min(currentIndex + 1, Devices.Count - 1);
            Devices.Move(currentIndex, newIndex);

            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void SendToBack(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            Devices.Move(Devices.IndexOf(surfaceDeviceViewModel), 0);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void SendBackward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            var currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            var newIndex = Math.Max(currentIndex - 1, 0);
            Devices.Move(currentIndex, newIndex);
            for (var i = 0; i < Devices.Count; i++)
            {
                var deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public async Task ViewProperties(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            var madeChanges = await _dialogService.ShowDialog<SurfaceDeviceConfigViewModel>(new Dictionary<string, object>
            {
                {"surfaceDeviceViewModel", surfaceDeviceViewModel}
            });

            if ((bool) madeChanges)
                _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        #endregion

        #region Selection

        private MouseDragStatus _mouseDragStatus;
        private Point _mouseDragStartPoint;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                ((IInputElement) sender).CaptureMouse();
            else
                ((IInputElement) sender).ReleaseMouseCapture();

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
            ((Grid) sender).Focus();
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
                _rgbService.IsRenderPaused = true;
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
                _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);

            _mouseDragStatus = MouseDragStatus.None;
            _rgbService.IsRenderPaused = false;
        }

        private void UpdateSelection(Point position)
        {
            if (IsPanKeyDown())
                return;

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
    }

    internal enum MouseDragStatus
    {
        None,
        Selecting,
        Dragging
    }
}