using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.UploadHandlers;
using ReactiveUI;
using StrawberryShake;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Shared.Routing;
using System;
using Artemis.UI.Shared.Utilities;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class UploadStepViewModel : SubmissionViewModel
{
    private readonly IWorkshopClient _workshopClient;
    private readonly EntryUploadHandlerFactory _entryUploadHandlerFactory;
    private readonly IWindowService _windowService;
    private readonly IRouter _router;
    private readonly Progress<StreamProgress> _progress = new();
    private readonly ObservableAsPropertyHelper<int> _progressPercentage;
    private readonly ObservableAsPropertyHelper<bool> _progressIndeterminate;

    private bool _finished;
    private Guid? _entryId;

    /// <inheritdoc />
    public UploadStepViewModel(IWorkshopClient workshopClient, EntryUploadHandlerFactory entryUploadHandlerFactory, IWindowService windowService, IRouter router)
    {
        _workshopClient = workshopClient;
        _entryUploadHandlerFactory = entryUploadHandlerFactory;
        _windowService = windowService;
        _router = router;

        ShowGoBack = false;
        ContinueText = "Finish";
        Continue = ReactiveCommand.CreateFromTask(ExecuteContinue, this.WhenAnyValue(vm => vm.Finished));
        
        _progressPercentage = Observable.FromEventPattern<StreamProgress>(x => _progress.ProgressChanged += x, x => _progress.ProgressChanged -= x)
            .Select(e => e.EventArgs.ProgressPercentage)
            .ToProperty(this, vm => vm.ProgressPercentage);
        _progressIndeterminate = Observable.FromEventPattern<StreamProgress>(x => _progress.ProgressChanged += x, x => _progress.ProgressChanged -= x)
            .Select(e => e.EventArgs.ProgressPercentage == 0)
            .ToProperty(this, vm => vm.ProgressIndeterminate);
        
        this.WhenActivated(d => Observable.FromAsync(ExecuteUpload).Subscribe().DisposeWith(d));
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    public int ProgressPercentage => _progressPercentage.Value;
    public bool ProgressIndeterminate => _progressIndeterminate.Value;

    public bool Finished
    {
        get => _finished;
        set => RaiseAndSetIfChanged(ref _finished, value);
    }

    public async Task ExecuteUpload(CancellationToken cancellationToken)
    {
        IOperationResult<IAddEntryResult> result = await _workshopClient.AddEntry.ExecuteAsync(new CreateEntryInput
        {
            EntryType = State.EntryType,
            Name = State.Name,
            Summary = State.Summary,
            Description = State.Description,
            Categories = State.Categories,
            Tags = State.Tags
        }, cancellationToken);

        Guid? entryId = result.Data?.AddEntry?.Id;
        if (result.IsErrorResult() || entryId == null)
        {
            await _windowService.ShowConfirmContentDialog("Failed to create workshop entry", result.Errors.ToString() ?? "Not even an error message", "Close", null);
            return;
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        // Create the workshop entry
        try
        {
            IEntryUploadHandler uploadHandler = _entryUploadHandlerFactory.CreateHandler(State.EntryType);
            EntryUploadResult uploadResult = await uploadHandler.CreateReleaseAsync(entryId.Value, State.EntrySource!, _progress, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                string? message = uploadResult.Message;
                if (message != null)
                    message += "\r\n\r\n";
                else
                    message = "";
                message += "Your submission has still been saved, you may try to upload a new release";
                await _windowService.ShowConfirmContentDialog("Failed to upload workshop entry", message, "Close", null);
                return;
            }

            _entryId = entryId;
            Finished = true;
        }
        catch (Exception e)
        {
            // Something went wrong when creating a release :c
            // We'll keep the workshop entry so that the user can make changes and try again
        }
    }

    private async Task ExecuteContinue()
    {
        if (_entryId == null)
            return;

        State.Finish();
        switch (State.EntryType)
        {
            case EntryType.Layout:
                await _router.Navigate($"workshop/layouts/{_entryId.Value}");
                break;
            case EntryType.Plugin:
                await _router.Navigate($"workshop/plugins/{_entryId.Value}");
                break;
            case EntryType.Profile:
                await _router.Navigate($"workshop/profiles/{_entryId.Value}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}