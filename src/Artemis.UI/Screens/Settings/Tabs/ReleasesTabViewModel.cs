using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Updating;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Settings;

public class ReleasesTabViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    private readonly IUpdatingClient _updatingClient;
    private readonly INotificationService _notificationService;
    private readonly SourceList<IGetReleases_PublishedReleases_Nodes> _releases;
    private IGetReleases_PublishedReleases_PageInfo? _lastPageInfo;
    private bool _loading;
    private ReleaseViewModel? _selectedReleaseViewModel;

    public ReleasesTabViewModel(ILogger logger, IUpdateService updateService, IUpdatingClient updatingClient, IReleaseVmFactory releaseVmFactory, INotificationService notificationService)
    {
        _logger = logger;
        _updatingClient = updatingClient;
        _notificationService = notificationService;

        _releases = new SourceList<IGetReleases_PublishedReleases_Nodes>();
        _releases.Connect()
            .Sort(SortExpressionComparer<IGetReleases_PublishedReleases_Nodes>.Descending(p => p.CreatedAt))
            .Transform(r => releaseVmFactory.ReleaseListViewModel(r.Id, r.Version, r.CreatedAt))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out ReadOnlyObservableCollection<ReleaseViewModel> releaseViewModels)
            .Subscribe();

        DisplayName = "Releases";
        ReleaseViewModels = releaseViewModels;
        this.WhenActivated(async d =>
        {
            await updateService.CacheLatestRelease();
            await GetMoreReleases(d.AsCancellationToken());
            SelectedReleaseViewModel = ReleaseViewModels.FirstOrDefault();
        });
    }

    public ReadOnlyObservableCollection<ReleaseViewModel> ReleaseViewModels { get; }

    public ReleaseViewModel? SelectedReleaseViewModel
    {
        get => _selectedReleaseViewModel;
        set => RaiseAndSetIfChanged(ref _selectedReleaseViewModel, value);
    }

    public bool Loading
    {
        get => _loading;
        private set => RaiseAndSetIfChanged(ref _loading, value);
    }

    public async Task GetMoreReleases(CancellationToken cancellationToken)
    {
        if (_lastPageInfo != null && !_lastPageInfo.HasNextPage)
            return;

        try
        {
            Loading = true;

            IOperationResult<IGetReleasesResult> result = await _updatingClient.GetReleases.ExecuteAsync("feature/gh-actions", Platform.Windows, 20, _lastPageInfo?.EndCursor, cancellationToken);
            if (result.Data?.PublishedReleases?.Nodes == null)
                return;

            _lastPageInfo = result.Data.PublishedReleases.PageInfo;
            _releases.AddRange(result.Data.PublishedReleases.Nodes);
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve releases");
            _notificationService.CreateNotification()
                .WithTitle("Failed to retrieve releases")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Warning)
                .Show();
        }
        finally
        {
            Loading = false;
        }
    }
}