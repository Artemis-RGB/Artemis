using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services;
using Avalonia;
using Avalonia.Media.Imaging;
using Material.Icons;
using RGB.NET.Core;
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

        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.Layout).Select(p => p != null));

        this.WhenAnyValue(vm => vm.SelectedDevice).WhereNotNull().Subscribe(d => Layout = d.Layout);
        this.WhenAnyValue(vm => vm.Layout).Subscribe(CreatePreviewDevice);
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

    private void ExecuteContinue()
    {
        if (Layout == null)
            return;

        State.EntrySource = Layout;
        State.Name = Layout.RgbLayout.Name ?? "";
        State.Summary = !string.IsNullOrWhiteSpace(Layout.RgbLayout.Vendor)
            ? $"{Layout.RgbLayout.Vendor} {Layout.RgbLayout.Type} device layout"
            : $"{Layout.RgbLayout.Type} device layout";

        // Go through the hassle of resizing the image to 128x128 without losing aspect ratio, padding is added for this
        using RenderTargetBitmap image = Layout.RenderLayout(false);
        using MemoryStream stream = new();
        image.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        MemoryStream output = new();
        using SKBitmap? sourceBitmap = SKBitmap.Decode(stream);
        int sourceWidth = sourceBitmap.Width;
        int sourceHeight = sourceBitmap.Height;
        float scale = Math.Min((float) 128 / sourceWidth, (float) 128 / sourceHeight);

        SKSizeI scaledDimensions = new((int) Math.Floor(sourceWidth * scale), (int) Math.Floor(sourceHeight * scale));
        SKPointI offset = new((128 - scaledDimensions.Width) / 2, (128 - scaledDimensions.Height) / 2);

        using (SKBitmap? scaleBitmap = sourceBitmap.Resize(scaledDimensions, SKFilterQuality.High))
        using (SKBitmap targetBitmap = new(128, 128))
        using (SKCanvas canvas = new(targetBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(scaleBitmap, offset.X, offset.Y);
            targetBitmap.Encode(output, SKEncodedImageFormat.Png, 100);
            output.Seek(0, SeekOrigin.Begin);
        }

        State.Icon?.Dispose();
        State.Icon = output;
        State.ChangeScreen<SpecificationsStepViewModel>();
    }
}