using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor.Dialogs;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using SkiaSharp;
using Stylet;
using MouseButton = System.Windows.Input.MouseButton;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceEditorViewModel : Screen, IMainScreenViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private readonly IInputService _inputService;
        private readonly IRgbService _rgbService;
        private readonly ISettingsService _settingsService;
        private Cursor _cursor;
        private PanZoomViewModel _panZoomViewModel;
        private RectangleGeometry _selectionRectangle;
        private PluginSetting<GridLength> _surfaceListWidth;

        public SurfaceEditorViewModel(IRgbService rgbService,
            ICoreService coreService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IDeviceService deviceService,
            IInputService inputService)
        {
            DisplayName = "Surface Editor";
            SelectionRectangle = new RectangleGeometry();
            PanZoomViewModel = new PanZoomViewModel();
            Cursor = null;

            SurfaceDeviceViewModels = new BindableCollection<SurfaceDeviceViewModel>();
            ListDeviceViewModels = new BindableCollection<ListDeviceViewModel>();

            _rgbService = rgbService;
            _coreService = coreService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _deviceService = deviceService;
            _inputService = inputService;
        }

        public BindableCollection<SurfaceDeviceViewModel> SurfaceDeviceViewModels { get; }
        public BindableCollection<ListDeviceViewModel> ListDeviceViewModels { get; }

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

        public void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Core.Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }

        public async Task AutoArrange()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog("Auto-arrange layout", "Are you sure you want to auto-arrange your layout? Your current settings will be overwritten.");
            if (!confirmed)
                return;

            _rgbService.AutoArrangeDevices();
        }

        private void LoadWorkspaceSettings()
        {
            SurfaceListWidth = _settingsService.GetSetting("SurfaceEditor.SurfaceListWidth", new GridLength(300.0));
        }

        private void SaveWorkspaceSettings()
        {
            SurfaceListWidth.Save();
        }


        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            foreach (ListDeviceViewModel listDeviceViewModel in ListDeviceViewModels)
            foreach (ArtemisLed artemisLed in listDeviceViewModel.Device.Leds)
                e.Canvas.DrawRect(artemisLed.AbsoluteRectangle, new SKPaint {Color = listDeviceViewModel.Color});
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            LoadWorkspaceSettings();
            SurfaceDeviceViewModels.AddRange(_rgbService.EnabledDevices.OrderBy(d => d.ZIndex).Select(d => new SurfaceDeviceViewModel(d, _rgbService)));
            ListDeviceViewModels.AddRange(_rgbService.EnabledDevices.OrderBy(d => d.ZIndex * -1).Select(d => new ListDeviceViewModel(d)));

            List<ArtemisDevice> shuffledDevices = _rgbService.EnabledDevices.OrderBy(d => Guid.NewGuid()).ToList();
            float amount = 360f / shuffledDevices.Count;
            for (int i = 0; i < shuffledDevices.Count; i++)
            {
                ArtemisDevice rgbServiceDevice = shuffledDevices[i];
                ListDeviceViewModel vm = ListDeviceViewModels.First(l => l.Device == rgbServiceDevice);
                vm.Color = SKColor.FromHsv(amount * i, 100, 100);
            }

            _coreService.FrameRendering += CoreServiceOnFrameRendering;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            SaveWorkspaceSettings();
            SurfaceDeviceViewModels.Clear();
            ListDeviceViewModels.Clear();

            _coreService.FrameRendering -= CoreServiceOnFrameRendering;

            base.OnClose();
        }

        #endregion

        #region Context menu actions

        public void IdentifyDevice(ArtemisDevice device)
        {
            _deviceService.IdentifyDevice(device);
        }

        public void BringToFront(ArtemisDevice device)
        {
            SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
            SurfaceDeviceViewModels.Move(SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel), SurfaceDeviceViewModels.Count - 1);
            for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

            _rgbService.SaveDevices();
        }

        public void BringForward(ArtemisDevice device)
        {
            SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
            int currentIndex = SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel);
            int newIndex = Math.Min(currentIndex + 1, SurfaceDeviceViewModels.Count - 1);
            SurfaceDeviceViewModels.Move(currentIndex, newIndex);

            for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

            _rgbService.SaveDevices();
        }

        public void SendToBack(ArtemisDevice device)
        {
            SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
            SurfaceDeviceViewModels.Move(SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel), 0);
            for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

            _rgbService.SaveDevices();
        }

        public void SendBackward(ArtemisDevice device)
        {
            SurfaceDeviceViewModel surfaceDeviceViewModel = SurfaceDeviceViewModels.First(d => d.Device == device);
            int currentIndex = SurfaceDeviceViewModels.IndexOf(surfaceDeviceViewModel);
            int newIndex = Math.Max(currentIndex - 1, 0);
            SurfaceDeviceViewModels.Move(currentIndex, newIndex);
            for (int i = 0; i < SurfaceDeviceViewModels.Count; i++)
            {
                SurfaceDeviceViewModel deviceViewModel = SurfaceDeviceViewModels[i];
                deviceViewModel.Device.ZIndex = i + 1;
            }

            ListDeviceViewModels.Sort(l => l.Device.ZIndex * -1);

            _rgbService.SaveDevices();
        }

        public async Task ViewProperties(ArtemisDevice device)
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceConfigViewModel>(
                new Dictionary<string, object> {{"device", device}}
            );

            if ((bool) madeChanges)
                _rgbService.SaveDevice(device);
        }

        public async Task DetectInput(ArtemisDevice device)
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceDetectInputViewModel>(
                new Dictionary<string, object> {{"device", device}}
            );

            if ((bool) madeChanges)
                _rgbService.SaveDevice(device);
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
            RectangleGeometry selectedRect = new(new Rect(position, new Size(1, 1)));
            SurfaceDeviceViewModel device = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, selectedRect).FirstOrDefault();
            if (device != null)
            {
                _rgbService.IsRenderPaused = true;
                _mouseDragStatus = MouseDragStatus.Dragging;
                // If the device is not selected, deselect others and select only this one (if shift not held)
                if (device.SelectionStatus != SelectionStatus.Selected)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        foreach (SurfaceDeviceViewModel others in SurfaceDeviceViewModels)
                            others.SelectionStatus = SelectionStatus.None;

                    device.SelectionStatus = SelectionStatus.Selected;
                }

                foreach (SurfaceDeviceViewModel selectedDevice in SurfaceDeviceViewModels.Where(d => d.SelectionStatus == SelectionStatus.Selected))
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
            ApplySurfaceSelection();
        }

        private void StopMouseDrag(object sender, Point position)
        {
            if (_mouseDragStatus != MouseDragStatus.Dragging)
            {
                RectangleGeometry selectedRect = new(new Rect(_mouseDragStartPoint, position));
                List<SurfaceDeviceViewModel> devices = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, selectedRect);
                foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels)
                    if (devices.Contains(device))
                        device.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        device.SelectionStatus = SelectionStatus.None;
            }
            else
            {
                _rgbService.SaveDevices();
            }

            _mouseDragStatus = MouseDragStatus.None;
            _rgbService.IsRenderPaused = false;
            ApplySurfaceSelection();
        }

        private void UpdateSelection(object sender, Point position)
        {
            if (IsPanKeyDown())
                return;

            Rect selectedRect = new(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            List<SurfaceDeviceViewModel> devices = HitTestUtilities.GetHitViewModels<SurfaceDeviceViewModel>((Visual) sender, SelectionRectangle);
            foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels)
                if (devices.Contains(device))
                    device.SelectionStatus = SelectionStatus.Selected;
                else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    device.SelectionStatus = SelectionStatus.None;

            ApplySurfaceSelection();
        }

        private void MoveSelected(Point position)
        {
            foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels.Where(d => d.SelectionStatus == SelectionStatus.Selected))
                device.UpdateMouseDrag(position);
        }

        private void ApplySurfaceSelection()
        {
            foreach (ListDeviceViewModel viewModel in ListDeviceViewModels)
                viewModel.IsSelected = SurfaceDeviceViewModels.Any(s => s.Device == viewModel.Device && s.SelectionStatus == SelectionStatus.Selected);
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