using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Updating;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Settings.Updating;

public class ReleaseAvailableViewModel : ActivatableViewModelBase
{
    private readonly string _nextReleaseId;
    private readonly ILogger _logger;
    private readonly IUpdateService _updateService;
    private readonly IUpdatingClient _updatingClient;
    private readonly INotificationService _notificationService;
    private IGetReleaseById_Release? _release;

    public ReleaseAvailableViewModel(string nextReleaseId, ILogger logger, IUpdateService updateService, IUpdatingClient updatingClient, INotificationService notificationService)
    {
        _nextReleaseId = nextReleaseId;
        _logger = logger;
        _updateService = updateService;
        _updatingClient = updatingClient;
        _notificationService = notificationService;

        CurrentVersion = _updateService.CurrentVersion ?? "Development build";
        Install = ReactiveCommand.Create(ExecuteInstall, this.WhenAnyValue(vm => vm.Release).Select(r => r != null));
        
        this.WhenActivated(async d => await RetrieveRelease(d.AsCancellationToken()));
    }

    private void ExecuteInstall()
    {
        _updateService.InstallRelease(_nextReleaseId);
    }

    private async Task RetrieveRelease(CancellationToken cancellationToken)
    {
        IOperationResult<IGetReleaseByIdResult> result = await _updatingClient.GetReleaseById.ExecuteAsync(_nextReleaseId, cancellationToken);
        // Borrow GraphQLClientException for messaging, how lazy of me..
        if (result.Errors.Count > 0)
        {
            GraphQLClientException exception = new(result.Errors);
            _logger.Error(exception, "Failed to retrieve release details");
            _notificationService.CreateNotification().WithTitle("Failed to retrieve release details").WithMessage(exception.Message).Show();
            return;
        }

        if (result.Data?.Release == null)
        {
            _notificationService.CreateNotification().WithTitle("Failed to retrieve release details").WithMessage("Release not found").Show();
            return;
        }

        Release = result.Data.Release;
    }
    
    public string CurrentVersion { get; }

    public IGetReleaseById_Release? Release
    {
        get => _release;
        set => RaiseAndSetIfChanged(ref _release, value);
    }

    public ReactiveCommand<Unit, Unit> Install { get; }
    public ReactiveCommand<Unit, Unit> AskLater { get; }
}