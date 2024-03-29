﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Artemis.Core;
using Artemis.UI.Screens.Device.Layout.LayoutProviders;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using RGB.NET.Layout;

namespace Artemis.UI.Screens.Device.Layout;

public partial class DeviceLayoutTabViewModel : ActivatableViewModelBase
{
    [Notify] private ILayoutProviderViewModel? _selectedLayoutProvider;

    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;

    public DeviceLayoutTabViewModel(IWindowService windowService, INotificationService notificationService, ArtemisDevice device, List<ILayoutProviderViewModel> layoutProviders)
    {
        _windowService = windowService;
        _notificationService = notificationService;

        Device = device;
        DisplayName = "Layout";
        DefaultLayoutPath = Device.DeviceProvider.LoadLayout(Device).FilePath;

        LayoutProviders = new ObservableCollection<ILayoutProviderViewModel>(layoutProviders);
        foreach (ILayoutProviderViewModel layoutProviderViewModel in layoutProviders)
        {
            layoutProviderViewModel.Device = Device;
            if (layoutProviderViewModel.Provider.IsMatch(Device))
                SelectedLayoutProvider = layoutProviderViewModel;
        }

        // When changing device provider to one that isn't currently on the device, apply it to the device immediately
        this.WhenAnyValue(vm => vm.SelectedLayoutProvider).Subscribe(l =>
        {
            if (l != null && !l.Provider.IsMatch(Device))
                l.Apply();
        });
    }

    public ArtemisDevice Device { get; }
    public ObservableCollection<ILayoutProviderViewModel> LayoutProviders { get; set; }

    public string DefaultLayoutPath { get; }

    public string? ImagePath => Device.Layout?.Image?.LocalPath;

    public async Task ExportLayout()
    {
        string fileName = Device.DeviceProvider.GetDeviceLayoutName(Device);
        string layoutDir = Constants.LayoutsFolder;
        string filePath = Path.Combine(
            layoutDir,
            Device.RgbDevice.DeviceInfo.Manufacturer,
            Device.DeviceType.ToString(),
            fileName
        );
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("xml").WithName("Artemis layout"))
            .WithDirectory(filePath)
            .WithInitialFileName(fileName)
            .ShowAsync();

        if (result == null)
            return;

        ArtemisLayout? layout = Device.Layout;
        if (layout?.IsValid == true)
        {
            string path = layout.FilePath;
            File.Copy(path, result, true);
        }
        else
        {
            List<LedLayout> ledLayouts = Device.Leds.Select(x => new LedLayout
            {
                Id = x.RgbLed.Id.ToString(),
                DescriptiveX = x.Rectangle.Left.ToString(),
                DescriptiveY = x.Rectangle.Top.ToString(),
                DescriptiveWidth = $"{x.Rectangle.Width}mm",
                DescriptiveHeight = $"{x.Rectangle.Height}mm"
            }).ToList();

            DeviceLayout emptyLayout = new()
            {
                Author = "Artemis",
                Type = Device.DeviceType,
                Vendor = Device.RgbDevice.DeviceInfo.Manufacturer,
                Model = Device.RgbDevice.DeviceInfo.Model,
                Width = Device.Rectangle.Width,
                Height = Device.Rectangle.Height,
                InternalLeds = ledLayouts
            };

            XmlSerializer serializer = new(typeof(DeviceLayout));
            await using StreamWriter writer = new(result);
            serializer.Serialize(writer, emptyLayout);
        }

        _notificationService.CreateNotification()
            .WithMessage("Layout exported")
            .WithTimeout(TimeSpan.FromSeconds(5))
            .WithSeverity(NotificationSeverity.Success)
            .Show();
    }

    public void ShowCopiedNotification()
    {
        _notificationService.CreateNotification()
            .WithTitle("Copied!")
            .WithSeverity(NotificationSeverity.Informational)
            .Show();
    }
}