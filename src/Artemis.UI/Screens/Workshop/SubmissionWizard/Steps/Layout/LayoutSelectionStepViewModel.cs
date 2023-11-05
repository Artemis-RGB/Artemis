using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Models;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SkiaSharp;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;

public partial class LayoutSelectionStepViewModel : SubmissionViewModel
{
    private readonly IWindowService _windowService;
    [Notify] private ArtemisDevice? _selectedDevice;
    [Notify] private ArtemisLayout? _layout;
    [Notify] private Bitmap? _layoutImage;

    public LayoutSelectionStepViewModel(IDeviceService deviceService, IWindowService windowService)
    {
        _windowService = windowService;
        Devices = new ObservableCollection<ArtemisDevice>(
            deviceService.Devices
                .Where(d => d.Layout != null && d.Layout.IsValid)
                .DistinctBy(d => d.Layout?.FilePath)
                .OrderBy(d => d.RgbDevice.DeviceInfo.Model)
        );

        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<EntryTypeStepViewModel>());
        Continue = ReactiveCommand.CreateFromTask(ExecuteContinue, this.WhenAnyValue(vm => vm.Layout).Select(p => p != null));

        this.WhenAnyValue(vm => vm.SelectedDevice).WhereNotNull().Subscribe(d => Layout = d.Layout);
        this.WhenAnyValue(vm => vm.Layout).Subscribe(CreatePreviewDevice);

        this.WhenActivated((CompositeDisposable _) =>
        {
            ShowGoBack = State.EntryId == null;
            if (State.EntrySource is not LayoutEntrySource layoutEntrySource)
                return;
            Layout = layoutEntrySource.Layout;
            SelectedDevice = Devices.FirstOrDefault(d => d.Layout == Layout);
        });
    }

    public ObservableCollection<ArtemisDevice> Devices { get; }

    public async Task BrowseLayout()
    {
        string[]? selected = await _windowService.CreateOpenFileDialog().HavingFilter(f => f.WithExtension("xml").WithName("Artemis Layout")).ShowAsync();
        if (selected == null || selected.Length != 1)
            return;

        try
        {
            ArtemisLayout layout = new(selected[0], LayoutSource.User);
            if (!layout.IsValid)
            {
                await _windowService.ShowConfirmContentDialog("Failed to load layout", "The selected file does not appear to be a valid RGB.NET layout file.", "Close", null);
                return;
            }

            SelectedDevice = null;
            Layout = layout;
        }
        catch (Exception e)
        {
            await _windowService.ShowConfirmContentDialog("Failed to load layout", "The selected file does not appear to be a valid RGB.NET layout file.\r\n" + e.Message, "Close", null);
            throw;
        }
    }

    private void CreatePreviewDevice(ArtemisLayout? layout)
    {
        if (layout == null)
        {
            LayoutImage = null;
            return;
        }

        LayoutImage = layout.RenderLayout(true);
        Layout = layout;
    }

    private async Task ExecuteContinue()
    {
        if (Layout == null)
            return;
        if (!await ValidateLayout(Layout))
            return;

        State.EntrySource = new LayoutEntrySource(Layout);
        await Dispatcher.UIThread.InvokeAsync(SetDeviceImages, DispatcherPriority.Background);
        State.ChangeScreen<LayoutInfoStepViewModel>();
    }

    private async Task<bool> ValidateLayout(ArtemisLayout? layout)
    {
        if (layout == null)
            return true;
        string? layoutPath = Path.GetDirectoryName(layout.FilePath);
        if (layoutPath == null)
            throw new ArtemisUIException($"Could not determine directory of {layout.FilePath}");

        if (layout.LayoutCustomDeviceData.DeviceImage != null && !File.Exists(Path.Combine(layoutPath, layout.LayoutCustomDeviceData.DeviceImage)))
        {
            await _windowService.ShowConfirmContentDialog(
                "Device image not found",
                $"{Path.Combine(layoutPath, layout.LayoutCustomDeviceData.DeviceImage)} does not exist.",
                "Close",
                null
            );
            return false;
        }

        foreach (ArtemisLedLayout ledLayout in layout.Leds)
        {
            if (ledLayout.LayoutCustomLedData.LogicalLayouts == null)
                continue;

            foreach (LayoutCustomLedDataLogicalLayout customData in ledLayout.LayoutCustomLedData.LogicalLayouts)
            {
                if (customData.Image == null || File.Exists(Path.Combine(layoutPath, customData.Image)))
                    continue;
                await _windowService.ShowConfirmContentDialog(
                    $"Image not found for LED {ledLayout.RgbLayout.Id} ({customData.Name})",
                    $"{Path.Combine(layoutPath, customData.Image)} does not exist.",
                    "Close",
                    null
                );
                return false;
            }
        }

        return true;
    }
    
    private void SetDeviceImages()
    {
        if (Layout == null)
            return;

        MemoryStream deviceWithoutLeds = new();
        MemoryStream deviceWithLeds = new();

        using (RenderTargetBitmap image = Layout.RenderLayout(false))
        {
            image.Save(deviceWithoutLeds);
            deviceWithoutLeds.Seek(0, SeekOrigin.Begin);
        }

        using (RenderTargetBitmap image = Layout.RenderLayout(true))
        {
            image.Save(deviceWithLeds);
            deviceWithLeds.Seek(0, SeekOrigin.Begin);
        }

        State.Icon?.Dispose();
        foreach (ImageUploadRequest stateImage in State.Images)
            stateImage.File.Dispose();
        State.Images.Clear();

        // Go through the hassle of resizing the image to 128x128 without losing aspect ratio, padding is added for this
        State.Icon = ResizeImage(deviceWithoutLeds, 128);
        State.Images.Add(new ImageUploadRequest(deviceWithoutLeds, "Layout preview (no LEDs)", "A preview of the device without its LEDs"));
        State.Images.Add(new ImageUploadRequest(deviceWithLeds, "Layout preview (with LEDs)", "A preview of the device with its LEDs"));
    }

    private Stream ResizeImage(Stream image, int size)
    {
        MemoryStream output = new();
        using MemoryStream input = new();

        image.CopyTo(input);
        input.Seek(0, SeekOrigin.Begin);

        using SKBitmap? sourceBitmap = SKBitmap.Decode(input);
        int sourceWidth = sourceBitmap.Width;
        int sourceHeight = sourceBitmap.Height;
        float scale = Math.Min((float) size / sourceWidth, (float) size / sourceHeight);

        SKSizeI scaledDimensions = new((int) Math.Floor(sourceWidth * scale), (int) Math.Floor(sourceHeight * scale));
        SKPointI offset = new((size - scaledDimensions.Width) / 2, (size - scaledDimensions.Height) / 2);

        using SKBitmap? scaleBitmap = sourceBitmap.Resize(scaledDimensions, SKFilterQuality.High);
        using SKBitmap targetBitmap = new(size, size);
        using SKCanvas canvas = new(targetBitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(scaleBitmap, offset.X, offset.Y);
        targetBitmap.Encode(output, SKEncodedImageFormat.Png, 100);
        output.Seek(0, SeekOrigin.Begin);

        return output;
    }
}