using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class UploadStepViewModel : SubmissionViewModel
{
    private readonly ILogger _logger;
    private readonly EntryUploadHandlerFactory _entryUploadHandlerFactory;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly IWorkshopClient _workshopClient;
    private readonly IWorkshopService _workshopService;
    private long? _entryId;
    [Notify] private bool _failed;
    [Notify] private bool _finished;
    [Notify] private bool _succeeded;

    /// <inheritdoc />
    public UploadStepViewModel(ILogger logger,
        IWorkshopClient workshopClient,
        IWorkshopService workshopService,
        EntryUploadHandlerFactory entryUploadHandlerFactory,
        IWindowService windowService,
        IRouter router)
    {
        _logger = logger;
        _workshopClient = workshopClient;
        _workshopService = workshopService;
        _entryUploadHandlerFactory = entryUploadHandlerFactory;
        _windowService = windowService;
        _router = router;

        ShowGoBack = false;
        ContinueText = "Finish";
        Continue = ReactiveCommand.CreateFromTask(ExecuteContinue, this.WhenAnyValue(vm => vm.Finished));

        this.WhenActivated(d => Observable.FromAsync(ExecuteUpload).Subscribe().DisposeWith(d));
    }

    private async Task ExecuteUpload(CancellationToken cancellationToken)
    {
        // Use the existing entry or create a new one
        _entryId = State.EntryId ?? await CreateEntry(cancellationToken);

        // If a new entry had to be created but that failed, stop here, CreateEntry will send the user back
        if (_entryId == null)
            return;

        try
        {
            IEntryUploadHandler uploadHandler = _entryUploadHandlerFactory.CreateHandler(State.EntryType);
            EntryUploadResult uploadResult = await uploadHandler.CreateReleaseAsync(_entryId.Value, State.EntrySource!, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                string? message = uploadResult.Message;
                if (message != null)
                    message += "\r\n\r\n";
                else
                    message = "";
                message += "Your submission has still been saved, you may try to upload a new release";
                await _windowService.ShowConfirmContentDialog("Failed to upload workshop entry", message, "Close", null);
            }

            Succeeded = true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to upload submission for entry {EntryId}", _entryId);

            // Something went wrong when creating a release :c
            // We'll keep the workshop entry so that the user can make changes and try again
            Failed = true;
        }
        finally
        {
            Finished = true;
        }
    }

    private async Task<long?> CreateEntry(CancellationToken cancellationToken)
    {
        await Task.Delay(2000);
        
        IOperationResult<IAddEntryResult> result = await _workshopClient.AddEntry.ExecuteAsync(new CreateEntryInput
        {
            EntryType = State.EntryType,
            Name = State.Name,
            Summary = State.Summary,
            Description = State.Description,
            Categories = State.Categories,
            Tags = State.Tags
        }, cancellationToken);

        long? entryId = result.Data?.AddEntry?.Id;
        if (result.IsErrorResult() || entryId == null)
        {
            await _windowService.ShowConfirmContentDialog("Failed to create workshop entry", string.Join("\r\n", result.Errors.Select(e => e.Message)), "Close", null);
            State.ChangeScreen<SubmitStepViewModel>();
            return null;
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ImageUploadRequest image in State.Images.ToList())
        {
            await TryImageUpload(async () => await _workshopService.UploadEntryImage(entryId.Value, image, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (State.Icon == null)
            return entryId;

        // Upload icon
        await TryImageUpload(async () => await _workshopService.SetEntryIcon(entryId.Value, State.Icon, cancellationToken));
       
        return entryId;
    }

    private async Task TryImageUpload(Func<Task<ImageUploadResult>> action)
    {
        try
        {
            ImageUploadResult result = await action();
            if (!result.IsSuccess)
                throw new ArtemisWorkshopException(result.Message);
        }
        catch (Exception e)
        {
            // It's not critical if this fails
            await _windowService.ShowConfirmContentDialog("Failed to upload", "Your submission will continue, you can try upload a new image afterwards\r\n" + e.Message, "Continue", null);
        }
    }

    private async Task ExecuteContinue()
    {
        State.Close();

        if (_entryId != null)
            await _router.Navigate($"workshop/library/submissions/{_entryId.Value}");
    }
}