using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Image;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class ImagesStepViewModel : SubmissionViewModel
{
    private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB
    private readonly IWindowService _windowService;
    private readonly Func<ImageUploadRequest, ImageSubmissionViewModel> _getImageSubmissionViewModel;
    private readonly SourceList<ImageUploadRequest> _stateImages;

    public ImagesStepViewModel(IWindowService windowService, Func<ImageUploadRequest, ImageSubmissionViewModel> getImageSubmissionViewModel)
    {
        _windowService = windowService;
        _getImageSubmissionViewModel = getImageSubmissionViewModel;

        Continue = ReactiveCommand.Create(() => State.ChangeScreen<UploadStepViewModel>());
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<SpecificationsStepViewModel>());
        Secondary = ReactiveCommand.CreateFromTask(ExecuteAddImage);
        SecondaryText = "Add image";

        _stateImages = new SourceList<ImageUploadRequest>();
        _stateImages.Connect()
            .Transform(p => CreateImageSubmissionViewModel(p))
            .Bind(out ReadOnlyObservableCollection<ImageSubmissionViewModel> images)
            .Subscribe();
        Images = images;

        this.WhenActivated((CompositeDisposable d) =>
        {
            _stateImages.Clear();
            _stateImages.AddRange(State.Images);
        });
    }

    public ReadOnlyObservableCollection<ImageSubmissionViewModel> Images { get; }

    private ImageSubmissionViewModel CreateImageSubmissionViewModel(ImageUploadRequest image)
    {
        ImageSubmissionViewModel viewModel = _getImageSubmissionViewModel(image);
        viewModel.Remove = ReactiveCommand.Create(() => RemoveImage(image));
        return viewModel;
    }

    private async Task ExecuteAddImage(CancellationToken arg)
    {
        string[]? result = await _windowService.CreateOpenFileDialog().WithAllowMultiple().HavingFilter(f => f.WithBitmaps()).ShowAsync();
        if (result == null)
            return;

        foreach (string path in result)
        {
            if (_stateImages.Items.Any(i => i.File is FileStream fs && fs.Name == path))
                continue;

            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            if (stream.Length > MAX_FILE_SIZE)
            {
                await _windowService.ShowConfirmContentDialog("File too big", $"File {path} exceeds maximum file size of 10 MB", "Skip file", null);
                await stream.DisposeAsync();
                continue;
            }

            ImageUploadRequest request = new(stream, Path.GetFileName(path), string.Empty);
            AddImage(request);

            // Show the dialog to give the image a name and description
            if (await Images.Last().Edit() != ContentDialogResult.Primary)
                RemoveImage(request); // user did not click confirm, remove again
        }
    }

    private void AddImage(ImageUploadRequest image)
    {
        _stateImages.Add(image);
        State.Images.Add(image);
    }

    private void RemoveImage(ImageUploadRequest image)
    {
        _stateImages.Remove(image);
        State.Images.Remove(image);
    }
}