using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImageSubmissionViewModel : ValidatableViewModelBase
{
    private readonly ImageUploadRequest _image;
    private readonly IWindowService _windowService;
    [Notify(Setter.Private)] private Bitmap? _bitmap;
    [Notify(Setter.Private)] private string? _imageDimensions;
    [Notify(Setter.Private)] private long _fileSize;
    [Notify] private string? _name;
    [Notify] private string? _description;
    [Notify] private ICommand? _remove;

    public ImageSubmissionViewModel(ImageUploadRequest image, IWindowService windowService)
    {
        _image = image;
        _windowService = windowService;
        
        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _image.File.Seek(0, SeekOrigin.Begin);
                Bitmap = new Bitmap(_image.File);
                FileSize = _image.File.Length;
                ImageDimensions = Bitmap.Size.Width + "x" + Bitmap.Size.Height;
                Name = _image.Name;
                Description = _image.Description;

                Bitmap.DisposeWith(d);
            }, DispatcherPriority.Background);
        });
    }

    public async Task Edit()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Edit image properties")
            .WithViewModel(out ImagePropertiesDialogViewModel vm, _image)
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();
    }
}