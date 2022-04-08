using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Avalonia;
using Avalonia.Skia;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceEditorViewModel : MainScreenViewModel
    {
        private readonly IRgbService _rgbService;
        private readonly ISettingsService _settingsService;
        private bool _saving;

        public SurfaceEditorViewModel(IScreen hostScreen,
            IRgbService rgbService,
            ISurfaceVmFactory surfaceVmFactory,
            ISettingsService settingsService) : base(hostScreen, "surface-editor")
        {
            _rgbService = rgbService;
            _settingsService = settingsService;
            DisplayName = "Surface Editor";
            SurfaceDeviceViewModels = new ObservableCollection<SurfaceDeviceViewModel>(rgbService.Devices.OrderBy(d => d.ZIndex).Select(surfaceVmFactory.SurfaceDeviceViewModel));
            ListDeviceViewModels = new ObservableCollection<ListDeviceViewModel>(rgbService.Devices.OrderBy(d => d.ZIndex).Select(surfaceVmFactory.ListDeviceViewModel));

            BringToFront = ReactiveCommand.Create<ArtemisDevice>(ExecuteBringToFront);
            BringForward = ReactiveCommand.Create<ArtemisDevice>(ExecuteBringForward);
            SendToBack = ReactiveCommand.Create<ArtemisDevice>(ExecuteSendToBack);
            SendBackward = ReactiveCommand.Create<ArtemisDevice>(ExecuteSendBackward);
        }

        public ObservableCollection<SurfaceDeviceViewModel> SurfaceDeviceViewModels { get; }
        public ObservableCollection<ListDeviceViewModel> ListDeviceViewModels { get; }

        public ReactiveCommand<ArtemisDevice, Unit> BringToFront { get; }
        public ReactiveCommand<ArtemisDevice, Unit> BringForward { get; }
        public ReactiveCommand<ArtemisDevice, Unit> SendToBack { get; }
        public ReactiveCommand<ArtemisDevice, Unit> SendBackward { get; }

        public double MaxTextureSize => 4096 / _settingsService.GetSetting("Core.RenderScale", 0.25).Value;

        public void ClearSelection()
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
                surfaceDeviceViewModel.SelectionStatus = SelectionStatus.None;
        }

        public void StartMouseDrag(Point mousePosition)
        {
            SurfaceDeviceViewModel? startedOn = GetViewModelAtPoint(mousePosition);
            if (startedOn != null && startedOn.SelectionStatus != SelectionStatus.Selected)
            {
                startedOn.SelectionStatus = SelectionStatus.Selected;
                foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels.Where(vm => vm != startedOn))
                    device.SelectionStatus = SelectionStatus.None;
            }

            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
                surfaceDeviceViewModel.StartMouseDrag(mousePosition);
        }

        public void UpdateMouseDrag(Point mousePosition, bool round, bool ignoreOverlap)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
                surfaceDeviceViewModel.UpdateMouseDrag(mousePosition, round, ignoreOverlap);
        }

        public void StopMouseDrag(Point mousePosition, bool round, bool ignoreOverlap)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
                surfaceDeviceViewModel.UpdateMouseDrag(mousePosition, round, ignoreOverlap);

            if (_saving)
                return;

            Task.Run(() =>
            {
                try
                {
                    _saving = true;
                    _rgbService.SaveDevices();
                }
                finally
                {
                    _saving = false;
                }
            });
        }

        public void SelectFirstDeviceAtPoint(Point point, bool expand)
        {
            SurfaceDeviceViewModel? toSelect = GetViewModelAtPoint(point);
            if (toSelect != null)
                toSelect.SelectionStatus = SelectionStatus.Selected;

            if (!expand)
            {
                foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels.Where(vm => vm != toSelect))
                    device.SelectionStatus = SelectionStatus.None;
            }

            ApplySurfaceSelection();
        }

        private SurfaceDeviceViewModel? GetViewModelAtPoint(Point point)
        {
            SKPoint hitTestPoint = point.ToSKPoint();
            return SurfaceDeviceViewModels.OrderByDescending(vm => vm.Device.ZIndex).FirstOrDefault(d => d.Device.Rectangle.Contains(hitTestPoint));
        }

        public void UpdateSelection(Rect rect, bool expand)
        {
            SKRect hitTestRect = rect.ToSKRect();
            foreach (SurfaceDeviceViewModel device in SurfaceDeviceViewModels)
            {
                if (device.Device.Rectangle.IntersectsWith(hitTestRect))
                    device.SelectionStatus = SelectionStatus.Selected;
                else if (!expand)
                    device.SelectionStatus = SelectionStatus.None;
            }

            ApplySurfaceSelection();
        }

        private void ApplySurfaceSelection()
        {
            foreach (ListDeviceViewModel viewModel in ListDeviceViewModels)
                viewModel.IsSelected = SurfaceDeviceViewModels.Any(s => s.Device == viewModel.Device && s.SelectionStatus == SelectionStatus.Selected);
        }

        #region Context menu commands

        private void ExecuteBringToFront(ArtemisDevice device)
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

        private void ExecuteBringForward(ArtemisDevice device)
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

        private void ExecuteSendToBack(ArtemisDevice device)
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

        private void ExecuteSendBackward(ArtemisDevice device)
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

        #endregion
    }
}