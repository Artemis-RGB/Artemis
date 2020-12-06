using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor.Dialogs;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;
using MouseButton = System.Windows.Input.MouseButton;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceEditorViewModel : Screen, IMainScreenViewModel
    {
        private readonly IDeviceService _deviceService;
        private readonly IInputService _inputService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private Cursor _cursor;
        private ObservableCollection<SurfaceDeviceViewModel> _devices;
        private PanZoomViewModel _panZoomViewModel;
        private ArtemisSurface _selectedSurface;
        private RectangleGeometry _selectionRectangle;
        private ObservableCollection<ArtemisSurface> _surfaceConfigurations;
        private PluginSetting<GridLength> _surfaceListWidth;

        public SurfaceEditorViewModel(IRgbService rgbService,
            ISurfaceService surfaceService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IDeviceService deviceService,
            IInputService inputService)
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
            _inputService = inputService;
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
            ArtemisSurface config = _surfaceService.CreateSurfaceConfiguration(name);
            Execute.PostToUIThread(() => SurfaceConfigurations.Add(config));
            return config;
        }

        public async Task AutoArrange()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog("Auto-arrange layout", "Are you sure you want to auto-arrange your layout? Your current settings will be overwritten.");
            if (!confirmed)
                return;
            
            _surfaceService.AutoArrange();

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
            List<ArtemisSurface> configs = _surfaceService.SurfaceConfigurations.ToList();

            // Get the active config, if empty, create a default config
            ArtemisSurface activeConfig = _surfaceService.ActiveSurface;
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
                foreach (ArtemisSurface surfaceConfiguration in configs)
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
                    List<SurfaceDeviceViewModel> existing = Devices.ToList();
                    List<SurfaceDeviceViewModel> deviceViewModels = new List<SurfaceDeviceViewModel>();

                    // Add missing/update existing
                    foreach (ArtemisDevice surfaceDeviceConfiguration in SelectedSurface.Devices.OrderBy(d => d.ZIndex).ToList())
                    {
                        // Create VMs for missing devices
                        SurfaceDeviceViewModel viewModel = existing.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
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
            bool result = await _dialogService.ShowConfirmDialogAt(
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
            object result = await _dialogService.ShowDialogAt<SurfaceCreateViewModel>("SurfaceListDialogHost");
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
            for (int i = 0; i < Devices.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void BringForward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            int currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            int newIndex = Math.Min(currentIndex + 1, Devices.Count - 1);
            Devices.Move(currentIndex, newIndex);

            for (int i = 0; i < Devices.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void SendToBack(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            Devices.Move(Devices.IndexOf(surfaceDeviceViewModel), 0);
            for (int i = 0; i < Devices.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public void SendBackward(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            int currentIndex = Devices.IndexOf(surfaceDeviceViewModel);
            int newIndex = Math.Max(currentIndex - 1, 0);
            Devices.Move(currentIndex, newIndex);
            for (int i = 0; i < Devices.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = Devices[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public async Task ViewProperties(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceConfigViewModel>(
                new Dictionary<string, object> {{"device", surfaceDeviceViewModel.Device}}
            );

            if ((bool) madeChanges)
                _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
        }

        public async Task DetectInput(SurfaceDeviceViewModel surfaceDeviceViewModel)
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceDetectInputViewModel>(
                new Dictionary<string, object> {{"device", surfaceDeviceViewModel.Device}}
            );

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

            Point position = e.GetPosition((IInputElement) sender);
            Point relative = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (e.LeftButton == MouseButtonState.Pressed)
                StartMouseDrag(sender, position, relative);
            else
                StopMouseDrag(sender, position);
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

            Point position = e.GetPosition((IInputElement) sender);
            Point relative = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (_mouseDragStatus == MouseDragStatus.Dragging)
                MoveSelected(relative);
            else if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(sender, position);
        }

        private void StartMouseDrag(object sender, Point position, Point relative)
        {
            // If drag started on top of a device, initialise dragging
            RectangleGeometry selectedRect = new RectangleGeometry(new Rect(position, new Size(1, 1)));
            SurfaceDeviceViewModel device = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, selectedRect).FirstOrDefault();
            if (device != null)
            {
                _rgbService.IsRenderPaused = true;
                _mouseDragStatus = MouseDragStatus.Dragging;
                // If the device is not selected, deselect others and select only this one (if shift not held)
                if (device.SelectionStatus != SelectionStatus.Selected)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        foreach (SurfaceDeviceViewModel others in Devices)
                            others.SelectionStatus = SelectionStatus.None;

                    device.SelectionStatus = SelectionStatus.Selected;
                }

                foreach (SurfaceDeviceViewModel selectedDevice in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
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

        private void StopMouseDrag(object sender, Point position)
        {
            if (_mouseDragStatus != MouseDragStatus.Dragging)
            {
                RectangleGeometry selectedRect = new RectangleGeometry(new Rect(_mouseDragStartPoint, position));
                List<SurfaceDeviceViewModel> devices = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, selectedRect);
                foreach (SurfaceDeviceViewModel device in Devices)
                    if (devices.Contains(device))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        device.SelectionStatus = SelectionStatus.None;
            }
            else
            {
                _surfaceService.UpdateSurfaceConfiguration(SelectedSurface, true);
            }

            _mouseDragStatus = MouseDragStatus.None;
            _rgbService.IsRenderPaused = false;
        }

        private void UpdateSelection(object sender, Point position)
        {
            if (IsPanKeyDown())
                return;

            Rect selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            List<SurfaceDeviceViewModel> devices = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, SelectionRectangle);
            foreach (SurfaceDeviceViewModel device in Devices)
                if (devices.Contains(device))
                    device.SelectionStatus = SelectionStatus.Selected;
                else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    device.SelectionStatus = SelectionStatus.None;
        }

        private void MoveSelected(Point position)
        {
            foreach (SurfaceDeviceViewModel device in Devices.Where(d => d.SelectionStatus == SelectionStatus.Selected))
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