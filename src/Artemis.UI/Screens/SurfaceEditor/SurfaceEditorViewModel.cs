using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.SurfaceEditor;

public class SurfaceEditorViewModel : MainScreenViewModel
{
    private readonly IRgbService _rgbService;
    private readonly ISettingsService _settingsService;
    private List<SurfaceDeviceViewModel>? _initialSelection;
    private bool _saving;

    public SurfaceEditorViewModel(IScreen hostScreen,
        IRgbService rgbService,
        ISurfaceVmFactory surfaceVmFactory,
        ISettingsService settingsService) : base(hostScreen, "surface-editor")
    {
        _rgbService = rgbService;
        _settingsService = settingsService;
        DisplayName = "Surface Editor";
        SurfaceDeviceViewModels = new ObservableCollection<SurfaceDeviceViewModel>(rgbService.EnabledDevices.OrderBy(d => d.ZIndex).Select(d => surfaceVmFactory.SurfaceDeviceViewModel(d, this)));
        ListDeviceViewModels = new ObservableCollection<ListDeviceViewModel>(rgbService.EnabledDevices.OrderBy(d => d.ZIndex).Select(d => surfaceVmFactory.ListDeviceViewModel(d, this)));

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

    public void UpdateSelection(List<SurfaceDeviceViewModel> devices, bool expand, bool invert)
    {
        _initialSelection ??= SurfaceDeviceViewModels.Where(d => d.IsSelected).ToList();

        if (expand)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = true;
        }
        else if (invert)
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = !_initialSelection.Contains(surfaceDeviceViewModel);
        }
        else
        {
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in devices)
                surfaceDeviceViewModel.IsSelected = true;
            foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels.Except(devices))
                surfaceDeviceViewModel.IsSelected = false;
        }
    }

    public void FinishSelection()
    {
        _initialSelection = null;

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

    public void ClearSelection()
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.IsSelected = false;
    }

    public void StartMouseDrag(Point mousePosition)
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.StartMouseDrag(mousePosition);
    }

    public void UpdateMouseDrag(Point mousePosition, bool round, bool ignoreOverlap)
    {
        foreach (SurfaceDeviceViewModel surfaceDeviceViewModel in SurfaceDeviceViewModels)
            surfaceDeviceViewModel.UpdateMouseDrag(mousePosition, round, ignoreOverlap);
    }
    
    private void ApplySurfaceSelection()
    {
        foreach (ListDeviceViewModel viewModel in ListDeviceViewModels)
            viewModel.IsSelected = SurfaceDeviceViewModels.Any(s => s.Device == viewModel.Device && s.IsSelected);
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