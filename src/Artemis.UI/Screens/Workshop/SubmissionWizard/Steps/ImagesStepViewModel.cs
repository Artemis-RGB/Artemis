using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Image;
using Artemis.UI.Shared.Services;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class ImagesStepViewModel : SubmissionViewModel
{
    private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB
    private readonly IWindowService _windowService;
    private readonly SourceList<Stream> _imageStreams;

    public ImagesStepViewModel(IWindowService windowService, Func<Stream, ImageSubmissionViewModel> imageSubmissionViewModel)
    {
        _windowService = windowService;

        Continue = ReactiveCommand.Create(() => State.ChangeScreen<UploadStepViewModel>());
        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<SpecificationsStepViewModel>());
        Secondary = ReactiveCommand.CreateFromTask(ExecuteAddImage);
        SecondaryText = "Add image";

        _imageStreams = new SourceList<Stream>();
        _imageStreams.Connect()
            .Transform(p => CreateImageSubmissionViewModel(imageSubmissionViewModel, p))
            .Bind(out ReadOnlyObservableCollection<ImageSubmissionViewModel> images)
            .Subscribe();
        Images = images;

        this.WhenActivated((CompositeDisposable d) =>
        {
            _imageStreams.Clear();
            _imageStreams.AddRange(State.Images);
        });
    }

    public ReadOnlyObservableCollection<ImageSubmissionViewModel> Images { get; }

    private ImageSubmissionViewModel CreateImageSubmissionViewModel(Func<Stream, ImageSubmissionViewModel> imageSubmissionViewModel, Stream stream)
    {
        ImageSubmissionViewModel viewModel = imageSubmissionViewModel(stream);
        viewModel.Remove = ReactiveCommand.Create(() => _imageStreams.Remove(stream));
        return viewModel;
    }

    private async Task ExecuteAddImage(CancellationToken arg)
    {
        string[]? result = await _windowService.CreateOpenFileDialog().WithAllowMultiple().HavingFilter(f => f.WithBitmaps()).ShowAsync();
        if (result == null)
            return;

        foreach (string path in result)
        {
            if (_imageStreams.Items.Any(i => i is FileStream fs && fs.Name == path))
                continue;

            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            if (stream.Length > MAX_FILE_SIZE)
            {
                await _windowService.ShowConfirmContentDialog("File too big", $"File {path} exceeds maximum file size of 10 MB", "Skip file", null);
                await stream.DisposeAsync();
                continue;
            }

            _imageStreams.Add(stream);
            State.Images.Add(stream);
        }
    }
}