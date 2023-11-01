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

        ArtemisLayout layout = new(selected[0], LayoutSource.User);
        if (!layout.IsValid)
        {
            await _windowService.ShowConfirmContentDialog("Invalid layout file", "The selected file does not appear to be a valid RGB.NET layout file", cancel: null);
            return;
        }

        SelectedDevice = null;
        Layout = layout;
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

        State.EntrySource = new LayoutEntrySource(Layout);
        await Dispatcher.UIThread.InvokeAsync(SetDeviceImages, DispatcherPriority.Background);
        State.ChangeScreen<LayoutInfoStepViewModel>();
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
        foreach (Stream stateImage in State.Images)
            stateImage.Dispose();
        State.Images.Clear();
        
        // Go through the hassle of resizing the image to 128x128 without losing aspect ratio, padding is added for this
        State.Icon = ResizeImage(deviceWithoutLeds, 128);
        State.Images.Add(deviceWithoutLeds);
        State.Images.Add(deviceWithLeds);
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