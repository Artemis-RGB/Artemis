using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ContentDialogButton = Artemis.UI.Shared.Services.Builders.ContentDialogButton;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImageSubmissionViewModel : ValidatableViewModelBase
{
    private readonly IWindowService _windowService;
    [Notify(Setter.Private)] private Bitmap? _bitmap;
    [Notify(Setter.Private)] private string? _imageDimensions;
    [Notify(Setter.Private)] private long _fileSize;
    [Notify] private string? _name;
    [Notify] private string? _description;
    [Notify] private bool _hasChanges;
    [Notify] private ICommand? _remove;

    public ImageSubmissionViewModel(ImageUploadRequest imageUploadRequest, IWindowService windowService)
    {
        ImageUploadRequest = imageUploadRequest;
        _windowService = windowService;

        FileSize = imageUploadRequest.File.Length;
        Name = imageUploadRequest.Name;
        Description = imageUploadRequest.Description;
        HasChanges = true;

        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                imageUploadRequest.File.Seek(0, SeekOrigin.Begin);
                Bitmap = new Bitmap(imageUploadRequest.File);
                ImageDimensions = Bitmap.Size.Width + "x" + Bitmap.Size.Height;
                Bitmap.DisposeWith(d);
            }, DispatcherPriority.Background);
        });
    }

    public ImageSubmissionViewModel(IImage existingImage, IWindowService windowService, IHttpClientFactory httpClientFactory)
    {
        _windowService = windowService;

        Id = existingImage.Id;
        Name = existingImage.Name;
        Description = existingImage.Description;

        // Download the image
        this.WhenActivated(d =>
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                HttpClient client = httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
                byte[] bytes = await client.GetByteArrayAsync($"/images/{existingImage.Id}.png");
                MemoryStream stream = new(bytes);

                Bitmap = new Bitmap(stream);
                FileSize = stream.Length;
                ImageDimensions = Bitmap.Size.Width + "x" + Bitmap.Size.Height;
                Bitmap.DisposeWith(d);
            }, DispatcherPriority.Background);
        });

        PropertyChanged += (_, args) => HasChanges = HasChanges || args.PropertyName == nameof(Name) || args.PropertyName == nameof(Description);
    }

    public ImageUploadRequest? ImageUploadRequest { get; }
    public Guid? Id { get; }

    public async Task<ContentDialogResult> Edit()
    {
        ContentDialogResult result = await _windowService.CreateContentDialog()
            .WithTitle("Edit image properties")
            .WithViewModel(out ImagePropertiesDialogViewModel vm, Name ?? string.Empty, Description ?? string.Empty)
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();

        Name = vm.Name;
        Description = vm.Description;

        return result;
    }
}