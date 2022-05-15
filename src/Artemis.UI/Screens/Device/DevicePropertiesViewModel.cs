using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using ReactiveUI;
using RGB.NET.Core;
using SkiaSharp;
using ArtemisLed = Artemis.Core.ArtemisLed;

namespace Artemis.UI.Screens.Device
{
    public class DevicePropertiesViewModel : DialogViewModelBase<object>
    {
        public DevicePropertiesViewModel(ArtemisDevice device, ICoreService coreService, IDeviceVmFactory deviceVmFactory)
        {
            Device = device;
            SelectedLeds = new ObservableCollection<ArtemisLed>();
            Tabs = new ObservableCollection<ActivatableViewModelBase>();

            Tabs.Add(deviceVmFactory.DevicePropertiesTabViewModel(device));
            Tabs.Add(deviceVmFactory.DeviceInfoTabViewModel(device));
            if (Device.DeviceType == RGBDeviceType.Keyboard)
                Tabs.Add(deviceVmFactory.InputMappingsTabViewModel(device, SelectedLeds));
            Tabs.Add(deviceVmFactory.DeviceLedsTabViewModel(device, SelectedLeds));

            this.WhenActivated(d =>
            {
                coreService.FrameRendering += CoreServiceOnFrameRendering;
                Disposable.Create(() => coreService.FrameRendering -= CoreServiceOnFrameRendering).DisposeWith(d);
            });

            ClearSelectedLeds = ReactiveCommand.Create(ExecuteClearSelectedLeds);
        }

        public ArtemisDevice Device { get; }
        public ObservableCollection<ArtemisLed> SelectedLeds { get; }
        public ObservableCollection<ActivatableViewModelBase> Tabs { get; }

        public ReactiveCommand<Unit, Unit> ClearSelectedLeds { get; }
        
        private void ExecuteClearSelectedLeds()
        {
            SelectedLeds.Clear();
        }

        private void CoreServiceOnFrameRendering(object? sender, FrameRenderingEventArgs e)
        {
            if (!SelectedLeds.Any())
                return;

            using SKPaint highlightPaint = new() {Color = SKColors.White};
            using SKPaint dimPaint = new() {Color = new SKColor(0, 0, 0, 192)};
            foreach (ArtemisLed artemisLed in Device.Leds)
                e.Canvas.DrawRect(artemisLed.AbsoluteRectangle, SelectedLeds.Contains(artemisLed) ? highlightPaint : dimPaint);
        }
    }
}